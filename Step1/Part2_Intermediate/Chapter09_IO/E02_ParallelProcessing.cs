using LanguageExt;
using LanguageExt.Common;
using LanguageExt101.Common;
using static LanguageExt.Prelude;

namespace LanguageExt101.Part2_Intermediate.Chapter09_IO;

/// <summary>
/// IO 모나드를 사용한 병렬 처리를 학습합니다.
///
/// 학습 목표:
/// - fork를 사용한 비동기 작업 분기
/// - awaitAll로 여러 작업 동시 실행
/// - awaitAny로 첫 번째 완료된 작업 가져오기
/// - 병렬 처리의 에러 핸들링
///
/// 핵심 개념:
/// 함수형 프로그래밍에서 병렬 처리는 부수 효과의 조합입니다.
/// IO 모나드는 이러한 병렬 작업을 안전하고 예측 가능하게 관리합니다.
/// </summary>
public static class E02_ParallelProcessing
{
    public static void Run()
    {
        MenuHelper.PrintHeader("Chapter 09-E02: 병렬 처리");

        // ============================================================
        // 1. 순차 처리 vs 병렬 처리
        // ============================================================
        MenuHelper.PrintSubHeader("1. 순차 처리 vs 병렬 처리");

        MenuHelper.PrintExplanation("동일한 작업을 순차와 병렬로 비교합니다.");
        MenuHelper.PrintBlankLines();

        // 느린 API 호출 시뮬레이션
        IO<string> FetchData(string endpoint, int delayMs) =>
            IO.lift(() =>
            {
                Console.WriteLine($"    [{DateTime.Now:HH:mm:ss.fff}] {endpoint} 시작...");
                Thread.Sleep(delayMs);
                Console.WriteLine($"    [{DateTime.Now:HH:mm:ss.fff}] {endpoint} 완료!");
                return $"{endpoint}의 데이터";
            });

        // 순차 처리
        Console.WriteLine("  [순차 처리]");
        var startSeq = DateTime.Now;
        var sequential =
            from a in FetchData("API-A", 500)
            from b in FetchData("API-B", 500)
            from c in FetchData("API-C", 500)
            select new[] { a, b, c };

        var seqResults = sequential.Run();
        var seqTime = (DateTime.Now - startSeq).TotalMilliseconds;
        MenuHelper.PrintResult("순차 처리 시간", $"{seqTime:F0}ms");
        MenuHelper.PrintBlankLines();

        // 병렬 처리
        Console.WriteLine("  [병렬 처리]");
        var startPar = DateTime.Now;
        var parallel = awaitAll(
            FetchData("API-A", 500),
            FetchData("API-B", 500),
            FetchData("API-C", 500)
        );

        var parResults = parallel.Run();
        var parTime = (DateTime.Now - startPar).TotalMilliseconds;
        MenuHelper.PrintResult("병렬 처리 시간", $"{parTime:F0}ms");
        MenuHelper.PrintResult("속도 향상", $"{seqTime / parTime:F1}배");

        // ============================================================
        // 2. Fork와 Await 패턴
        // ============================================================
        MenuHelper.PrintSubHeader("2. Fork와 Await 패턴");

        MenuHelper.PrintExplanation("fork()로 작업을 백그라운드에서 시작하고");
        MenuHelper.PrintExplanation("await로 결과를 기다립니다.");
        MenuHelper.PrintBlankLines();

        var forkExample =
            from forked in fork(FetchData("백그라운드작업", 300))
            from _ in IO.lift(() => { Console.WriteLine("    메인 스레드는 다른 작업 수행 중..."); return unit; })
            from __ in yieldFor(TimeSpan.FromMilliseconds(100))
            from ___ in IO.lift(() => { Console.WriteLine("    메인 스레드 작업 완료, 결과 대기..."); return unit; })
            from result in forked.Await
            select result;

        var forkResult = forkExample.Run();
        MenuHelper.PrintResult("Fork 결과", forkResult);

        // ============================================================
        // 3. 여러 데이터 소스 병렬 조회
        // ============================================================
        MenuHelper.PrintSubHeader("3. 실전: 여러 데이터 소스 병렬 조회");

        MenuHelper.PrintExplanation("실제 시나리오: 사용자 정보를 여러 서비스에서 조합");
        MenuHelper.PrintBlankLines();

        var getUserProfile = CreateGetUserProfile();
        var profile = getUserProfile.Run();

        MenuHelper.PrintResult("사용자 프로필", profile);

        // ============================================================
        // 4. 병렬 처리와 에러 핸들링
        // ============================================================
        MenuHelper.PrintSubHeader("4. 병렬 처리와 에러 핸들링");

        MenuHelper.PrintExplanation("일부 작업이 실패해도 나머지 결과를 활용합니다.");
        MenuHelper.PrintBlankLines();

        IO<string> FetchWithError(string name, bool shouldFail) =>
            IO.lift(() =>
            {
                if (shouldFail) throw new Exception($"{name} 실패!");
                Thread.Sleep(100);
                return $"{name} 성공";
            });

        // 개별적으로 에러 처리
        var safeParallel =
            from results in awaitAll(
                FetchWithError("서비스1", false).Try(),
                FetchWithError("서비스2", true).Try(),  // 실패
                FetchWithError("서비스3", false).Try()
            )
            select results;

        var safeResults = safeParallel.Run();
        Console.WriteLine("  개별 결과:");
        int i = 1;
        foreach (var result in safeResults)
        {
            result.Match(
                Succ: v => Console.WriteLine($"    서비스{i++}: {v}"),
                Fail: e => Console.WriteLine($"    서비스{i++}: 에러 - {e.Message}")
            );
        }

        // ============================================================
        // 5. awaitAny - 가장 빠른 응답 사용
        // ============================================================
        MenuHelper.PrintSubHeader("5. awaitAny - 가장 빠른 응답 사용");

        MenuHelper.PrintExplanation("여러 미러 서버 중 가장 빠른 응답을 사용합니다.");
        MenuHelper.PrintBlankLines();

        IO<string> FetchFromMirror(string mirror, int delayMs) =>
            IO.lift(() =>
            {
                Console.WriteLine($"    [{DateTime.Now:HH:mm:ss.fff}] {mirror}에서 요청 시작");
                Thread.Sleep(delayMs);
                return $"{mirror}에서 데이터 수신 (지연: {delayMs}ms)";
            });

        var fastest = awaitAny(
            FetchFromMirror("미러1(느림)", 800),
            FetchFromMirror("미러2(빠름)", 200),
            FetchFromMirror("미러3(보통)", 500)
        );

        var fastestResult = fastest.Run();
        MenuHelper.PrintResult("가장 빠른 응답", fastestResult);

        // ============================================================
        // 6. 배치 처리
        // ============================================================
        MenuHelper.PrintSubHeader("6. 실전: 배치 처리");

        MenuHelper.PrintExplanation("대량의 데이터를 병렬로 처리합니다.");
        MenuHelper.PrintBlankLines();

        var items = Seq(1, 2, 3, 4, 5, 6, 7, 8, 9, 10);

        IO<int> ProcessItem(int item) =>
            IO.lift(() =>
            {
                Thread.Sleep(50);  // 처리 시뮬레이션
                return item * 2;
            });

        var startBatch = DateTime.Now;
        var batchProcess = awaitAll(toSeq(items.Map(ProcessItem)));
        var batchResults = batchProcess.Run();
        var batchTime = (DateTime.Now - startBatch).TotalMilliseconds;

        MenuHelper.PrintResult("처리된 항목", string.Join(", ", batchResults));
        MenuHelper.PrintResult("총 처리 시간", $"{batchTime:F0}ms (10개 항목, 각 50ms)");
        MenuHelper.PrintResult("순차 시 예상 시간", "500ms");

        MenuHelper.PrintSuccess("병렬 처리 학습 완료!");
    }

    private static IO<string> CreateGetUserProfile()
    {
        // 여러 서비스에서 사용자 정보 조회
        IO<string> GetBasicInfo() => IO.lift(() =>
        {
            Thread.Sleep(100);
            return "홍길동(user123)";
        });

        IO<string> GetOrderHistory() => IO.lift(() =>
        {
            Thread.Sleep(150);
            return "주문 5건";
        });

        IO<string> GetPreferences() => IO.lift(() =>
        {
            Thread.Sleep(80);
            return "다크모드, 한국어";
        });

        return
            from results in awaitAll(GetBasicInfo(), GetOrderHistory(), GetPreferences())
            let basic = results[0]
            let orders = results[1]
            let prefs = results[2]
            select $"사용자: {basic}, {orders}, 설정: {prefs}";
    }
}
