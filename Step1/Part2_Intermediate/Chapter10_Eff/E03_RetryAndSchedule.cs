using LanguageExt;
using LanguageExt.Common;
using LanguageExt101.Common;
using static LanguageExt.Prelude;

namespace LanguageExt101.Part2_Intermediate.Chapter10_Eff;

/// <summary>
/// 재시도(Retry)와 에러 복구 패턴을 학습합니다.
///
/// 학습 목표:
/// - 재시도 로직의 함수형 구현
/// - 지수 백오프(Exponential Backoff)
/// - 폴백(Fallback) 패턴
/// - Circuit Breaker 개념
///
/// 핵심 개념:
/// 네트워크 호출, 외부 API 등 실패 가능한 작업에서
/// 재시도와 에러 복구는 필수적입니다.
/// 함수형 방식으로 이를 조합 가능하게 구현합니다.
/// </summary>
public static class E03_RetryAndSchedule
{
    private static int _attemptCount = 0;
    private static readonly Random _random = new();

    public static void Run()
    {
        MenuHelper.PrintHeader("Chapter 10-E03: 재시도와 에러 복구");

        // ============================================================
        // 1. 기본 재시도 패턴
        // ============================================================
        MenuHelper.PrintSubHeader("1. 기본 재시도 패턴");

        MenuHelper.PrintExplanation("실패할 수 있는 작업을 지정된 횟수만큼 재시도합니다.");
        MenuHelper.PrintBlankLines();

        _attemptCount = 0;
        var basicRetry = Retry(
            () => UnstableOperation("API 호출", 3),
            maxAttempts: 5,
            delayMs: 100
        );

        var result1 = basicRetry.Run();
        result1.Match(
            Succ: v => MenuHelper.PrintResult("성공", v),
            Fail: e => MenuHelper.PrintResult("최종 실패", e.Message)
        );

        // ============================================================
        // 2. 지수 백오프 재시도
        // ============================================================
        MenuHelper.PrintSubHeader("2. 지수 백오프 재시도");

        MenuHelper.PrintExplanation("실패할 때마다 대기 시간을 2배씩 늘립니다.");
        MenuHelper.PrintExplanation("서버 과부하 방지에 효과적입니다.");
        MenuHelper.PrintBlankLines();

        _attemptCount = 0;
        var exponentialRetry = RetryWithExponentialBackoff(
            () => UnstableOperation("DB 쿼리", 2),
            maxAttempts: 4,
            initialDelayMs: 50
        );

        var result2 = exponentialRetry.Run();
        result2.Match(
            Succ: v => MenuHelper.PrintResult("성공", v),
            Fail: e => MenuHelper.PrintResult("최종 실패", e.Message)
        );

        // ============================================================
        // 3. 조건부 재시도
        // ============================================================
        MenuHelper.PrintSubHeader("3. 조건부 재시도");

        MenuHelper.PrintExplanation("특정 예외만 재시도하고 나머지는 즉시 실패합니다.");
        MenuHelper.PrintBlankLines();

        _attemptCount = 0;
        var conditionalRetry = RetryWhen(
            () => RandomErrorOperation(),
            shouldRetry: ex => ex.Message.Contains("일시적"),
            maxAttempts: 5
        );

        var result3 = conditionalRetry.Run();
        result3.Match(
            Succ: v => MenuHelper.PrintResult("성공", v),
            Fail: e => MenuHelper.PrintResult("실패 (재시도 불가 에러)", e.Message)
        );

        // ============================================================
        // 4. 폴백(Fallback) 패턴
        // ============================================================
        MenuHelper.PrintSubHeader("4. 폴백(Fallback) 패턴");

        MenuHelper.PrintExplanation("주 작업이 실패하면 대체 작업을 실행합니다.");
        MenuHelper.PrintBlankLines();

        var withFallback = IO.lift(() =>
            {
                Console.WriteLine("    [Primary] 주 서버 호출 시도...");
                throw new Exception("주 서버 응답 없음");
#pragma warning disable CS0162 // 접근할 수 없는 코드
                return "주 서버 데이터";
#pragma warning restore CS0162
            })
            .Try()
            .IfFail(_ =>
            {
                Console.WriteLine("    [Fallback] 백업 서버 호출...");
                return "백업 서버 데이터";
            });

        var fallbackResult = withFallback.As().Run();
        MenuHelper.PrintResult("폴백 결과", fallbackResult);

        // ============================================================
        // 5. 다중 폴백 체인
        // ============================================================
        MenuHelper.PrintSubHeader("5. 다중 폴백 체인");

        MenuHelper.PrintExplanation("여러 대안을 순차적으로 시도합니다.");
        MenuHelper.PrintBlankLines();

        var fallbackChain = TryMultiple(
            ("Redis 캐시", () => { Console.WriteLine("    [1] Redis 캐시 조회..."); throw new Exception("Redis 연결 실패"); return "캐시 데이터"; }),
            ("PostgreSQL", () => { Console.WriteLine("    [2] PostgreSQL 조회..."); throw new Exception("DB 타임아웃"); return "DB 데이터"; }),
            ("로컬 파일", () => { Console.WriteLine("    [3] 로컬 파일 읽기..."); return "로컬 파일 데이터"; })
        );

        var chainResult = fallbackChain.Run();
        chainResult.Match(
            Succ: v => MenuHelper.PrintResult("체인 성공", v),
            Fail: e => MenuHelper.PrintResult("모든 대안 실패", e.Message)
        );

        // ============================================================
        // 6. 타임아웃 패턴
        // ============================================================
        MenuHelper.PrintSubHeader("6. 타임아웃 패턴");

        MenuHelper.PrintExplanation("지정된 시간 내에 완료되지 않으면 실패로 처리합니다.");
        MenuHelper.PrintBlankLines();

        var withTimeout = WithTimeout(
            () =>
            {
                Console.WriteLine("    [작업] 빠른 작업 실행...");
                Thread.Sleep(50);
                return "빠른 작업 완료";
            },
            timeoutMs: 200
        );

        var timeoutResult = withTimeout.Run();
        timeoutResult.Match(
            Succ: v => MenuHelper.PrintResult("타임아웃 내 완료", v),
            Fail: e => MenuHelper.PrintResult("타임아웃", e.Message)
        );

        // ============================================================
        // 7. Circuit Breaker 시뮬레이션
        // ============================================================
        MenuHelper.PrintSubHeader("7. Circuit Breaker 시뮬레이션");

        MenuHelper.PrintExplanation("연속 실패 시 일시적으로 요청을 차단합니다.");
        MenuHelper.PrintBlankLines();

        var breaker = new SimpleCircuitBreaker(failureThreshold: 3, resetTimeMs: 1000);

        for (int i = 1; i <= 5; i++)
        {
            var operation = breaker.Execute(() =>
            {
                if (i <= 3) throw new Exception($"실패 #{i}");
                return $"성공 #{i}";
            });

            operation.Run().Match(
                Succ: v => Console.WriteLine($"    요청 {i}: {v}"),
                Fail: e => Console.WriteLine($"    요청 {i}: {e.Message}")
            );
        }

        Console.WriteLine("    [대기] 회로 리셋 대기 중...");
        Thread.Sleep(1100);

        breaker.Execute(() => "회로 리셋 후 성공")
            .Run()
            .Match(
                Succ: v => MenuHelper.PrintResult("리셋 후", v),
                Fail: e => MenuHelper.PrintResult("리셋 후 실패", e.Message)
            );

        // ============================================================
        // 8. 실전: API 클라이언트
        // ============================================================
        MenuHelper.PrintSubHeader("8. 실전: 복원력 있는 API 클라이언트");

        MenuHelper.PrintExplanation("재시도, 타임아웃, 폴백을 조합한 실전 패턴입니다.");
        MenuHelper.PrintBlankLines();

        var resilientCall = CreateResilientApiCall(
            "https://api.example.com/users",
            maxRetries: 3,
            timeoutMs: 500
        );

        var apiResult = resilientCall.Run();
        apiResult.Match(
            Succ: v => MenuHelper.PrintResult("API 응답", v),
            Fail: e => MenuHelper.PrintResult("API 실패", e.Message)
        );

        MenuHelper.PrintSuccess("재시도와 에러 복구 학습 완료!");
    }

    // ============================================================
    // 헬퍼 함수들
    // ============================================================

    private static IO<Fin<string>> Retry(Func<string> operation, int maxAttempts, int delayMs)
    {
        return IO.lift<Fin<string>>(() =>
        {
            Exception? lastError = null;
            for (int attempt = 1; attempt <= maxAttempts; attempt++)
            {
                try
                {
                    Console.WriteLine($"    [시도 {attempt}/{maxAttempts}]");
                    var result = operation();
                    return Fin.Succ(result);
                }
                catch (Exception ex)
                {
                    lastError = ex;
                    Console.WriteLine($"    실패: {ex.Message}");
                    if (attempt < maxAttempts)
                    {
                        Console.WriteLine($"    {delayMs}ms 후 재시도...");
                        Thread.Sleep(delayMs);
                    }
                }
            }
            return Fin.Fail<string>(lastError!.Message);
        });
    }

    private static IO<Fin<string>> RetryWithExponentialBackoff(Func<string> operation, int maxAttempts, int initialDelayMs)
    {
        return IO.lift<Fin<string>>(() =>
        {
            Exception? lastError = null;
            int delay = initialDelayMs;

            for (int attempt = 1; attempt <= maxAttempts; attempt++)
            {
                try
                {
                    Console.WriteLine($"    [시도 {attempt}/{maxAttempts}]");
                    var result = operation();
                    return Fin.Succ(result);
                }
                catch (Exception ex)
                {
                    lastError = ex;
                    Console.WriteLine($"    실패: {ex.Message}");
                    if (attempt < maxAttempts)
                    {
                        Console.WriteLine($"    {delay}ms 후 재시도 (지수 백오프)...");
                        Thread.Sleep(delay);
                        delay *= 2; // 지수 증가
                    }
                }
            }
            return Fin.Fail<string>(lastError!.Message);
        });
    }

    private static IO<Fin<string>> RetryWhen(Func<string> operation, Func<Exception, bool> shouldRetry, int maxAttempts)
    {
        return IO.lift<Fin<string>>(() =>
        {
            Exception? lastError = null;
            for (int attempt = 1; attempt <= maxAttempts; attempt++)
            {
                try
                {
                    Console.WriteLine($"    [시도 {attempt}/{maxAttempts}]");
                    var result = operation();
                    return Fin.Succ(result);
                }
                catch (Exception ex)
                {
                    lastError = ex;
                    Console.WriteLine($"    예외: {ex.Message}");

                    if (!shouldRetry(ex))
                    {
                        Console.WriteLine("    재시도 불가능한 에러, 즉시 종료");
                        return Fin.Fail<string>(ex.Message);
                    }

                    if (attempt < maxAttempts)
                    {
                        Console.WriteLine("    재시도 가능, 계속 진행...");
                    }
                }
            }
            return Fin.Fail<string>(lastError!.Message);
        });
    }

    private static IO<Fin<string>> TryMultiple(params (string name, Func<string> action)[] alternatives)
    {
        return IO.lift<Fin<string>>(() =>
        {
            Exception? lastError = null;
            foreach (var (name, action) in alternatives)
            {
                try
                {
                    var result = action();
                    Console.WriteLine($"    [{name}] 성공!");
                    return Fin.Succ(result);
                }
                catch (Exception ex)
                {
                    lastError = ex;
                    Console.WriteLine($"    [{name}] 실패: {ex.Message}");
                }
            }
            return Fin.Fail<string>(lastError!.Message);
        });
    }

    private static IO<Fin<string>> WithTimeout(Func<string> operation, int timeoutMs)
    {
        return IO.lift<Fin<string>>(() =>
        {
            using var cts = new CancellationTokenSource(timeoutMs);
            var task = Task.Run(operation, cts.Token);

            try
            {
                if (task.Wait(timeoutMs))
                {
                    return Fin.Succ(task.Result);
                }
                return Fin.Fail<string>("작업 타임아웃");
            }
            catch (AggregateException ae)
            {
                return Fin.Fail<string>((ae.InnerException ?? ae).Message);
            }
        });
    }

    private static IO<Fin<string>> CreateResilientApiCall(string url, int maxRetries, int timeoutMs)
    {
        return IO.lift<Fin<string>>(() =>
        {
            Console.WriteLine($"    [API] {url} 호출 중...");
            Console.WriteLine($"    설정: 최대 {maxRetries}회 재시도, {timeoutMs}ms 타임아웃");

            // 시뮬레이션: 3번째 시도에서 성공
            _attemptCount = 0;
            Exception? lastError = null;

            for (int attempt = 1; attempt <= maxRetries; attempt++)
            {
                try
                {
                    _attemptCount++;
                    Console.WriteLine($"    [시도 {attempt}] 요청 전송...");

                    if (_attemptCount < 3)
                        throw new Exception("서버 일시 오류 (503)");

                    return Fin.Succ("{ \"users\": [...], \"count\": 42 }");
                }
                catch (Exception ex)
                {
                    lastError = ex;
                    Console.WriteLine($"    실패: {ex.Message}");
                    if (attempt < maxRetries)
                        Thread.Sleep(100);
                }
            }

            // 폴백: 캐시된 데이터 반환
            Console.WriteLine("    [폴백] 캐시된 데이터 사용");
            return Fin.Succ("{ \"users\": [...], \"count\": 35, \"cached\": true }");
        });
    }

    private static string UnstableOperation(string name, int failUntil)
    {
        _attemptCount++;
        if (_attemptCount < failUntil)
            throw new Exception($"{name} 일시적 오류 (시도 {_attemptCount})");
        return $"{name} 성공 (시도 {_attemptCount})";
    }

    private static string RandomErrorOperation()
    {
        _attemptCount++;
        var errorType = _random.Next(3);
        if (_attemptCount < 3)
        {
            if (errorType == 0)
                throw new Exception("일시적 네트워크 오류");
            else
                throw new Exception("영구적 인증 오류");
        }
        return $"작업 성공 (시도 {_attemptCount})";
    }

    // Circuit Breaker 구현
    private class SimpleCircuitBreaker
    {
        private int _failureCount;
        private readonly int _failureThreshold;
        private readonly int _resetTimeMs;
        private DateTime _lastFailure;
        private bool _isOpen;

        public SimpleCircuitBreaker(int failureThreshold, int resetTimeMs)
        {
            _failureThreshold = failureThreshold;
            _resetTimeMs = resetTimeMs;
        }

        public IO<Fin<string>> Execute(Func<string> operation)
        {
            return IO.lift<Fin<string>>(() =>
            {
                // 회로가 열려있는지 확인
                if (_isOpen)
                {
                    if ((DateTime.Now - _lastFailure).TotalMilliseconds < _resetTimeMs)
                    {
                        return Fin.Fail<string>("회로 차단 중 - 요청 거부");
                    }
                    // 리셋 시간 경과, 반열림 상태
                    _isOpen = false;
                    _failureCount = 0;
                }

                try
                {
                    var result = operation();
                    _failureCount = 0; // 성공 시 카운터 리셋
                    return Fin.Succ(result);
                }
                catch (Exception ex)
                {
                    _failureCount++;
                    _lastFailure = DateTime.Now;

                    if (_failureCount >= _failureThreshold)
                    {
                        _isOpen = true;
                        Console.WriteLine($"    [Circuit Breaker] 회로 열림! ({_failureCount}회 연속 실패)");
                    }

                    return Fin.Fail<string>(ex.Message);
                }
            });
        }
    }
}
