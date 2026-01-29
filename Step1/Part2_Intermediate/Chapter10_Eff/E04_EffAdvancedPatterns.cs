using LanguageExt;
using LanguageExt.Common;
using LanguageExt.Sys;
using LanguageExt.Sys.Traits;
using LanguageExt.Traits;
using LanguageExt101.Common;
using static LanguageExt.Prelude;
using static LanguageExt.UnitsOfMeasure;

namespace LanguageExt101.Part2_Intermediate.Chapter10_Eff;

/// <summary>
/// Eff의 고급 패턴을 학습합니다.
///
/// 학습 목표:
/// - Schedule 조합 (fibonacci, spaced, recurs)
/// - repeat와 retry 패턴
/// - guard와 @catch 에러 처리
/// - timeout과 cancel
/// - fork와 FoldIO
///
/// 핵심 개념:
/// language-ext는 효과의 반복, 재시도, 취소 등을 선언적으로 처리하는
/// 풍부한 연산자를 제공합니다. Schedule을 조합하여 복잡한 타이밍을
/// 간결하게 표현할 수 있습니다.
/// </summary>
public static class E04_EffAdvancedPatterns
{
    public static void Run()
    {
        MenuHelper.PrintHeader("Chapter 10-E04: Eff 고급 패턴");

        // ============================================================
        // 1. Schedule 조합
        // ============================================================
        MenuHelper.PrintSubHeader("1. Schedule 조합");

        MenuHelper.PrintExplanation("Schedule: 시간 간격 정책을 정의하는 타입");
        MenuHelper.PrintExplanation("spaced: 고정 간격");
        MenuHelper.PrintExplanation("fibonacci: 피보나치 간격 증가");
        MenuHelper.PrintExplanation("recurs: 반복 횟수 제한");
        MenuHelper.PrintExplanation("exponential: 지수 백오프");
        MenuHelper.PrintBlankLines();

        MenuHelper.PrintCode(@"// Schedule 생성
Schedule.spaced(1 * second)         // 1초 고정 간격
Schedule.fibonacci(100 * ms)        // 피보나치 증가 (100, 100, 200, 300, 500...)
Schedule.exponential(50 * ms)       // 지수 증가 (50, 100, 200, 400...)
Schedule.recurs(5)                  // 5회 반복");

        // Schedule 시뮬레이션
        var fibonacci = Schedule.fibonacci(100 * ms);
        var recurs = Schedule.recurs(5);

        MenuHelper.PrintBlankLines();
        MenuHelper.PrintCode(@"// Schedule 조합 (| 연산자)
var combined = Schedule.spaced(10 * second)
             | Schedule.fibonacci(1 * second)
             | Schedule.recurs(9);
// spaced는 최대 간격 제한, fibonacci는 증가 패턴, recurs는 횟수 제한");

        // 조합 예시 설명
        Console.WriteLine();
        Console.WriteLine("    [Schedule 조합 예시]");
        Console.WriteLine("    fibonacci(1s) | recurs(5) 간격:");
        Console.WriteLine("    -> 1초, 1초, 2초, 3초, 5초 (피보나치 패턴, 5회)");
        Console.WriteLine();
        Console.WriteLine("    spaced(3s) | fibonacci(1s) | recurs(5) 간격:");
        Console.WriteLine("    -> 1초, 1초, 2초, 3초, 3초 (최대 3초로 제한)");

        // ============================================================
        // 2. repeat 패턴
        // ============================================================
        MenuHelper.PrintSubHeader("2. repeat 패턴");

        MenuHelper.PrintExplanation("repeat: 성공할 때까지 또는 Schedule이 끝날 때까지 반복");
        MenuHelper.PrintExplanation("+repeat(...): 동일하지만 결과를 무시 (Unit 반환)");
        MenuHelper.PrintBlankLines();

        MenuHelper.PrintCode(@"// repeat 사용 예시 (EffectsExamples/TimeExample 참고)
+repeat(Schedule.spaced(10 * second) | Schedule.fibonacci(1 * second) | Schedule.recurs(9),
        from tm in Time<RT>.now
        from _1 in Console<RT>.writeLine(tm.ToLongTimeString())
        select unit);");

        // 시뮬레이션
        Console.WriteLine();
        Console.WriteLine("    [repeat 시뮬레이션]");
        var times = new[] { "10:00:00", "10:00:01", "10:00:02", "10:00:04", "10:00:07" };
        for (int i = 0; i < times.Length; i++)
        {
            Console.WriteLine($"    반복 {i + 1}: {times[i]}");
        }

        // IO를 사용한 실제 repeat 예시
        MenuHelper.PrintBlankLines();
        MenuHelper.PrintCode("// 간단한 repeat 예시 (실제 실행)");

        var counter = 0;
        var repeatEff = liftEff(() =>
        {
            counter++;
            Console.WriteLine($"    실행 #{counter}");
            return counter;
        });

        // repeat 대신 수동으로 반복 (교육 목적)
        for (int i = 0; i < 3; i++)
        {
            repeatEff.Run();
        }

        // ============================================================
        // 3. retry 패턴
        // ============================================================
        MenuHelper.PrintSubHeader("3. retry 패턴");

        MenuHelper.PrintExplanation("retry: 실패 시 재시도, 성공하면 중단");
        MenuHelper.PrintExplanation("Schedule로 재시도 간격과 횟수 제어");
        MenuHelper.PrintBlankLines();

        MenuHelper.PrintCode(@"// retry 사용 예시 (EffectsExamples/RetryExample 참고)
+retry(Schedule.recurs(5),
       from _  in Console<RT>.writeLine(""Say hello"")
       from t  in Console<RT>.readLine
       from e  in guard(t == ""hello"", Error.New(""Failed""))
       from m  in Console<RT>.writeLine(""Hi"")
       select unit);");

        // retry 시뮬레이션
        Console.WriteLine();
        Console.WriteLine("    [retry 시뮬레이션]");
        Console.WriteLine("    시도 1: 입력 'hi' -> 실패, 재시도...");
        Console.WriteLine("    시도 2: 입력 'bye' -> 실패, 재시도...");
        Console.WriteLine("    시도 3: 입력 'hello' -> 성공!");

        // 실제 retry 예시
        MenuHelper.PrintBlankLines();
        var retryCounter = 0;
        var retryableEff = liftEff<int>(() =>
        {
            retryCounter++;
            if (retryCounter < 3)
                throw new Exception($"실패 #{retryCounter}");
            return retryCounter;
        });

        // @catch로 재시도 시뮬레이션
        var withRetry = retryableEff | @catch(e => liftEff(() =>
        {
            Console.WriteLine($"    에러: {e.Message}, 재시도...");
            return retryableEff.Run().Match(Succ: x => x, Fail: _ => -1);
        }));

        Console.WriteLine("    [실제 retry 시뮬레이션]");
        retryCounter = 0;
        var result = withRetry.Run();
        MenuHelper.PrintResult("최종 결과", result);

        // ============================================================
        // 4. guard와 에러
        // ============================================================
        MenuHelper.PrintSubHeader("4. guard와 에러");

        MenuHelper.PrintExplanation("guard(조건, 에러): 조건 false시 에러로 실패");
        MenuHelper.PrintExplanation("guard(조건, () => throw): 예외를 던지는 방식");
        MenuHelper.PrintBlankLines();

        // ---------------------------------------------------------
        // guard와 LINQ의 관계:
        //
        // guard(bool, Error) → Guard<Error, Unit> 반환
        // Guard는 Eff가 아니므로 LINQ 쿼리에서 특별한 규칙이 필요합니다.
        //
        // [핵심 규칙]
        // LINQ 첫 번째 from에는 반드시 Eff가 와야 합니다.
        //
        // 이유: C# LINQ는 첫 번째 from의 타입을 기준으로 SelectMany를 결정합니다.
        // - Eff.SelectMany(Func<A, Guard>, ...) → 정의됨 ✅
        // - Guard.SelectMany → Eff<A>로 변환 불가 ❌
        //
        // [Eff<RT, A>에서의 사용]
        // Console<RT>.readLine 같은 메서드가 Eff<RT, string>을 반환하므로
        // 자연스럽게 Eff가 먼저 오게 됩니다.
        // ---------------------------------------------------------

        MenuHelper.PrintCode(@"// Eff<RT, A>와 guard: Console<RT>.readLine이 Eff를 반환하므로 자연스럽게 동작
from ln in Console<RT>.readLine                   // Eff<RT, string> - Eff가 먼저!
from _1 in guard(notEmpty(ln), UserExited)        // guard 사용 가능
from _2 in guard(ln != ""sys"", () => throw new SystemException())
from _3 in guard(ln != ""err"", () => throw new Exception())
select ln;");

        // ---------------------------------------------------------
        // Eff<A> (런타임 없는 버전)에서 guard 사용:
        //
        // Eff<A>만 사용할 때는 SuccessEff(value)로 시작하여
        // Eff 컨텍스트를 먼저 설정합니다.
        // ---------------------------------------------------------
        Eff<string> ValidateInput(string input) =>
            from inp in SuccessEff(input)  // Eff가 먼저 → 이후 guard 사용 가능
            from _1 in guard(notEmpty(inp), Error.New(100, "입력이 비어있습니다"))
            from _2 in guard(inp != "exit", Error.New(200, "종료 요청"))
            from _3 in guard(inp.Length <= 10, Error.New(300, "너무 깁니다"))
            select inp;

        var testInputs = new[] { "", "exit", "this is too long input", "valid" };
        Console.WriteLine();
        foreach (var input in testInputs)
        {
            var res = ValidateInput(input).Run();
            Console.WriteLine($"    입력 \"{input}\": {res}");
        }

        // ============================================================
        // 5. @catch 패턴
        // ============================================================
        MenuHelper.PrintSubHeader("5. @catch 패턴");

        MenuHelper.PrintExplanation("| @catch: 파이프라인 방식 에러 핸들링");
        MenuHelper.PrintExplanation("조건부 catch, 에러 변환, 폴백 등 지원");
        MenuHelper.PrintBlankLines();

        MenuHelper.PrintCode(@"// @catch 체인 (ErrorAndGuardExample 참고)
from result in riskyOperation
             | @catch(ex => ex.HasException<SystemException>(),
                      Console<RT>.writeLine(""system error""))
             | SafeError  // 기타 에러는 SafeError로 변환
select result;");

        // @catch 예시
        var dangerousOp = FailEff<string>(Error.New(500, "서버 에러"));

        var handled = dangerousOp
            | @catch(e => e.Code == 404, SuccessEff("Not Found 처리"))
            | @catch(e => e.Code == 500, SuccessEff("서버 에러 처리"))
            | @catch(e => SuccessEff("기타 에러 처리"));

        Console.WriteLine();
        MenuHelper.PrintResult("@catch 체인 결과", handled.Run());

        // 에러 변환
        var transformed = FailEff<int>(Error.New(100, "원본 에러"))
            | Error.New(999, "변환된 에러");  // 에러를 다른 에러로 변환

        MenuHelper.PrintResult("에러 변환", transformed.Run());

        // ============================================================
        // 6. timeout 패턴
        // ============================================================
        MenuHelper.PrintSubHeader("6. timeout 패턴");

        MenuHelper.PrintExplanation("timeout(duration, eff): 지정 시간 초과 시 실패");
        MenuHelper.PrintExplanation("Errors.TimedOut: 타임아웃 전용 에러");
        MenuHelper.PrintBlankLines();

        MenuHelper.PrintCode(@"// timeout 사용 예시 (TimeoutExample 참고)
from _ in timeout(60 * seconds, longRunning)
        | @catch(Errors.TimedOut, pure<Eff<RT>, Unit>(unit))
from __ in Console<RT>.writeLine(""done"")
select unit;");

        // timeout 시뮬레이션
        Console.WriteLine();
        Console.WriteLine("    [timeout 시뮬레이션]");
        Console.WriteLine("    작업: 60초 타임아웃으로 장기 실행 작업 시작");
        Console.WriteLine("    -> 타임아웃 발생 시 Errors.TimedOut");
        Console.WriteLine("    -> @catch로 정상 종료 처리");

        // 실제 timeout 예시 (짧은 시간)
        MenuHelper.PrintBlankLines();
        var quickOp = liftEff(() =>
        {
            Thread.Sleep(50);
            return "완료";
        });

        var timedOut = quickOp; // 실제 timeout은 Eff<RT,A>에서 사용
        MenuHelper.PrintResult("빠른 작업", timedOut.Run());

        // ============================================================
        // 7. cancel 패턴
        // ============================================================
        MenuHelper.PrintSubHeader("7. cancel 패턴");

        MenuHelper.PrintExplanation("cancel: CancellationToken을 트리거하여 취소");
        MenuHelper.PrintExplanation("localCancel: 로컬 취소 범위 생성");
        MenuHelper.PrintBlankLines();

        MenuHelper.PrintCode(@"// cancel 사용 예시 (CancelExample 참고)
+repeat(from k in Console<RT>.readKey
        from _ in k.Key == ConsoleKey.Enter
                      ? cancel       // Enter 누르면 취소
                      : unitIO
        from w in Console<RT>.write(k.KeyChar)
        select unit);");

        // cancel 시뮬레이션
        Console.WriteLine();
        Console.WriteLine("    [cancel 시뮬레이션]");
        Console.WriteLine("    입력: a, b, c, Enter");
        Console.WriteLine("    출력: abc (Enter에서 취소되어 반복 종료)");

        // localCancel 설명
        MenuHelper.PrintBlankLines();
        MenuHelper.PrintCode(@"// localCancel: 중첩된 취소 범위
from result in localCancel(innerEffect)  // 내부 효과만 취소 가능
             | @catch(Errors.Cancelled, handleCancellation)
select result;");

        // ============================================================
        // 8. fork 패턴
        // ============================================================
        MenuHelper.PrintSubHeader("8. fork 패턴");

        MenuHelper.PrintExplanation("fork: 효과를 백그라운드에서 실행");
        MenuHelper.PrintExplanation("반환된 Fork에서 .Cancel로 취소 가능");
        MenuHelper.PrintBlankLines();

        MenuHelper.PrintCode(@"// fork 사용 예시 (ForkCancelExample 참고)
+from frk in fork(inner)           // 백그라운드 실행 시작
 from key in Console<RT>.readKey   // 메인: 키 입력 대기
 from _1  in frk.Cancel            // 포크된 작업 취소
 from _2  in Console<RT>.writeLine(""done"")
 select unit;");

        // fork 시뮬레이션
        Console.WriteLine();
        Console.WriteLine("    [fork 시뮬레이션]");
        Console.WriteLine("    메인: readKey 대기");
        Console.WriteLine("    백그라운드: 10회 반복 작업 실행 중...");
        Console.WriteLine("    사용자: Enter 입력");
        Console.WriteLine("    -> 백그라운드 작업 취소됨");
        Console.WriteLine("    -> 출력: done");

        // ============================================================
        // 9. FoldIO 패턴
        // ============================================================
        MenuHelper.PrintSubHeader("9. FoldIO 패턴");

        MenuHelper.PrintExplanation("FoldIO: Schedule에 따라 효과를 반복하며 값을 누적");
        MenuHelper.PrintExplanation("fold + repeat의 결합");
        MenuHelper.PrintBlankLines();

        MenuHelper.PrintCode(@"// FoldIO 사용 예시 (ForkCancelExample 참고)
digit.FoldIO(Schedule.recurs(9) | Schedule.spaced(1 * second),
             0,
             (s, x) => s + x);

// digit: 1을 반환하는 효과
// 결과: 0 + 1 + 1 + ... = 10 (10회 반복)");

        // FoldIO 시뮬레이션
        Console.WriteLine();
        Console.WriteLine("    [FoldIO 시뮬레이션]");
        Console.WriteLine("    초기값: 0");
        Console.WriteLine("    반복 1: 0 + 1 = 1");
        Console.WriteLine("    반복 2: 1 + 1 = 2");
        Console.WriteLine("    ...");
        Console.WriteLine("    반복 10: 9 + 1 = 10");
        Console.WriteLine("    최종 결과: 10");

        // 실제 FoldIO 예시 (동기 버전으로 시뮬레이션)
        MenuHelper.PrintBlankLines();
        var sum = Enumerable.Range(1, 5).Aggregate(0, (acc, x) =>
        {
            var newAcc = acc + x;
            Console.WriteLine($"    누적: {acc} + {x} = {newAcc}");
            return newAcc;
        });
        MenuHelper.PrintResult("실제 누적 결과", sum);

        // ============================================================
        // 10. 실전 조합: 복합 패턴
        // ============================================================
        MenuHelper.PrintSubHeader("10. 실전 조합: 복합 패턴");

        MenuHelper.PrintExplanation("여러 패턴을 조합한 복원력 있는 효과");
        MenuHelper.PrintBlankLines();

        MenuHelper.PrintCode(@"// 복합 패턴 예시
static Eff<RT, string> resilientFetch<RT>()
    where RT : Has<Eff<RT>, ConsoleIO>, Has<Eff<RT>, TimeIO>
{
    return timeout(30 * seconds,
             retry(Schedule.exponential(100 * ms) | Schedule.recurs(5),
                   from _  in Console<RT>.writeLine(""Fetching..."")
                   from r  in fetchData<RT>()
                   from __ in guard(r.IsSuccess, Error.New(""Fetch failed""))
                   select r.Data))
         | @catch(Errors.TimedOut, pure<Eff<RT>, string>(""Timeout fallback""))
         | @catch(e => logAndFallback<RT>(e));
}");

        Console.WriteLine();
        Console.WriteLine("    [복합 패턴 흐름]");
        Console.WriteLine("    1. timeout: 30초 제한");
        Console.WriteLine("    2. retry: 지수 백오프로 최대 5회 재시도");
        Console.WriteLine("    3. guard: 성공 응답 검증");
        Console.WriteLine("    4. @catch(TimedOut): 타임아웃 시 폴백");
        Console.WriteLine("    5. @catch: 기타 에러 로깅 후 폴백");

        // 실전 예제: 단계별 처리
        MenuHelper.PrintBlankLines();
        MenuHelper.PrintCode("// 실전 데이터 처리 파이프라인");

        var sourceData = new[] { 1, 2, 3, -1, 4, 5 };
        var validatedData = sourceData.Where(n => n > 0).ToList();
        var pipelineSum = validatedData.Sum();

        // 시뮬레이션: 음수 발견 시 필터링
        Console.WriteLine($"    검증: 음수 발견 (-1), 필터링됨");
        Console.WriteLine($"    결과: 합계 {pipelineSum} (항목: {validatedData.Count}개)");

        MenuHelper.PrintResult("파이프라인 결과", $"합계: {pipelineSum} (항목: {validatedData.Count}개)");

        // 요약
        MenuHelper.PrintBlankLines();
        MenuHelper.PrintExplanation("=== 고급 패턴 요약 ===");
        Console.WriteLine("    Schedule: 시간 정책 (spaced, fibonacci, recurs, exponential)");
        Console.WriteLine("    repeat/retry: 반복/재시도 (Schedule과 조합)");
        Console.WriteLine("    guard: 조건부 실패");
        Console.WriteLine("    @catch: 에러 핸들링 파이프라인");
        Console.WriteLine("    timeout: 시간 제한");
        Console.WriteLine("    cancel/localCancel: 취소 처리");
        Console.WriteLine("    fork: 백그라운드 실행");
        Console.WriteLine("    FoldIO: 반복하며 누적");

        MenuHelper.PrintSuccess("Eff 고급 패턴 학습 완료!");
    }
}

// ============================================================
// 부록: 고급 패턴 예제 (제네릭 클래스)
// ============================================================

/// <summary>
/// Schedule 기반 반복 패턴 예제
/// </summary>
public static class ScheduledEffects<RT>
    where RT : Has<Eff<RT>, ConsoleIO>, Has<Eff<RT>, TimeIO>
{
    /// <summary>
    /// 피보나치 간격으로 시간 출력 (TimeExample 기반)
    /// </summary>
    public static Eff<RT, Unit> clockWithFibonacci =>
        repeat(Schedule.spaced(10 * second) | Schedule.fibonacci(1 * second) | Schedule.recurs(9),
               from tm in Time<RT>.now
               from _1 in LanguageExt.Sys.Console<RT>.writeLine(tm.ToLongTimeString())
               select unit).As();

    /// <summary>
    /// 지수 백오프 재시도 (RetryExample 기반)
    /// </summary>
    public static Eff<RT, Unit> retryWithBackoff(Eff<RT, Unit> action) =>
        retry(Schedule.exponential(100 * ms) | Schedule.recurs(5), action).As();
}

/// <summary>
/// 에러 처리 패턴 예제
/// </summary>
public static class ErrorHandlingEffects<RT>
    where RT : Has<Eff<RT>, ConsoleIO>
{
    static readonly Error UserExited = Error.New(100, "사용자 종료");
    static readonly Error SafeError = Error.New(200, "안전한 에러");

    /// <summary>
    /// 다중 guard와 @catch 조합 (ErrorAndGuardExample 기반)
    /// </summary>
    public static Eff<RT, Unit> interactWithGuards =>
        from result in askUser
                     | @catch(ex => ex.HasException<SystemException>(),
                              LanguageExt.Sys.Console<RT>.writeLine("시스템 에러"))
                     | SafeError
        from _ in LanguageExt.Sys.Console<RT>.writeLine("완료")
        select unit;

    static Eff<RT, Unit> askUser =>
        (repeat(Schedule.spaced(1 * second) | Schedule.recurs(3),
               from ln in LanguageExt.Sys.Console<RT>.readLine
               from _1 in ln.Length == 0 ? FailEff<RT, Unit>(UserExited) : SuccessEff<RT, Unit>(unit)
               from _2 in LanguageExt.Sys.Console<RT>.writeLine(ln)
               select unit)
      | @catch(UserExited, SuccessEff<RT, Unit>(unit))).As();
}

/// <summary>
/// Fork 패턴 예제
/// </summary>
public static class ForkEffects<RT>
    where RT : Has<Eff<RT>, ConsoleIO>, Has<Eff<RT>, TimeIO>
{
    /// <summary>
    /// Fork와 취소 (ForkCancelExample 기반)
    /// </summary>
    public static Eff<RT, Unit> forkAndCancel =>
        (from frk in fork(innerTask)
        from key in LanguageExt.Sys.Console<RT>.readKey
        from _1 in frk.Cancel
        from _2 in LanguageExt.Sys.Console<RT>.writeLine("작업 취소됨")
        select unit).As();

    static Eff<RT, Unit> innerTask =>
        from x in sum
        from _ in LanguageExt.Sys.Console<RT>.writeLine($"총합: {x}")
        select unit;

    static Eff<RT, int> sum =>
        digit.FoldIO(Schedule.recurs(9) | Schedule.spaced(1 * second), 0, (s, x) => s + x).As();

    static Eff<RT, int> digit =>
        from one in SuccessEff<RT, int>(1)
        from _ in LanguageExt.Sys.Console<RT>.writeLine("*")
        select one;
}

/// <summary>
/// Timeout 패턴 예제
/// </summary>
public static class TimeoutEffects<RT>
    where RT : Has<Eff<RT>, ConsoleIO>, Has<Eff<RT>, TimeIO>
{
    /// <summary>
    /// 타임아웃과 폴백 (TimeoutExample 기반)
    /// </summary>
    public static Eff<RT, Unit> withTimeout =>
        from _1 in timeout(60 * seconds, longRunning)
                 | @catch(Errors.TimedOut, pure<Eff<RT>, Unit>(unit))
        from _2 in LanguageExt.Sys.Console<RT>.writeLine("완료")
        select unit;

    static Eff<RT, Unit> longRunning =>
        (from tm in Time<RT>.now
         from _1 in LanguageExt.Sys.Console<RT>.writeLine(tm.ToLongTimeString())
         select unit)
        .RepeatIO(Schedule.fibonacci(1 * second)).As();
}
