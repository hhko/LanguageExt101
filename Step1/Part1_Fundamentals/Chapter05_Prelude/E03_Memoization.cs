using LanguageExt;
using LanguageExt101.Common;
using static LanguageExt.Prelude;

namespace LanguageExt101.Part1_Fundamentals.Chapter05_Prelude;

/// <summary>
/// 메모이제이션(Memoization)을 학습합니다.
///
/// 학습 목표:
/// - 메모이제이션의 개념
/// - memo() 함수 사용법
/// - 재귀 함수 메모이제이션
/// - 주의사항과 제한
///
/// 핵심 개념:
/// 메모이제이션은 함수 호출 결과를 캐싱하는 기법입니다.
/// 동일한 인자로 호출되면 캐시된 결과를 반환합니다.
/// 순수 함수(부수 효과 없는 함수)에만 적용해야 합니다.
///
/// memo()는 자동으로 함수를 메모이제이션된 버전으로 변환합니다.
/// </summary>
public static class E03_Memoization
{
    /// <summary>
    /// 예제를 실행합니다.
    /// </summary>
    public static void Run()
    {
        MenuHelper.PrintHeader("Chapter 05-E03: 메모이제이션");

        // ============================================================
        // 1. 메모이제이션 기본
        // ============================================================
        MenuHelper.PrintSubHeader("1. 메모이제이션 기본");

        MenuHelper.PrintExplanation("메모이제이션은 함수 결과를 캐싱합니다.");
        MenuHelper.PrintExplanation("같은 입력에 대해 재계산하지 않고 캐시된 값을 반환합니다.");
        MenuHelper.PrintBlankLines();

        // 일반 함수 (매번 계산)
        Func<int, int> slowSquare = x =>
        {
            Console.WriteLine($"    [계산 중] {x}의 제곱 계산...");
            Thread.Sleep(100);  // 시간 소요 시뮬레이션
            return x * x;
        };

        // 메모이제이션된 버전 (수동 캐싱 구현)
        var squareCache = new Dictionary<int, int>();
        Func<int, int> memoSquare = x =>
        {
            if (squareCache.TryGetValue(x, out var cached)) return cached;
            var result = slowSquare(x);
            squareCache[x] = result;
            return result;
        };

        MenuHelper.PrintCode("// 메모이제이션 없이 (매번 계산)");
        Console.WriteLine($"  slowSquare(5) = {slowSquare(5)}");
        Console.WriteLine($"  slowSquare(5) = {slowSquare(5)}");
        MenuHelper.PrintBlankLines();

        MenuHelper.PrintCode("// 메모이제이션 적용 (첫 번째만 계산)");
        Console.WriteLine($"  memoSquare(5) = {memoSquare(5)}");
        Console.WriteLine($"  memoSquare(5) = {memoSquare(5)}");

        // ============================================================
        // 2. 2인자 함수 메모이제이션
        // ============================================================
        MenuHelper.PrintSubHeader("2. 2인자 함수 메모이제이션");

        MenuHelper.PrintExplanation("여러 인자를 받는 함수도 메모이제이션할 수 있습니다.");
        MenuHelper.PrintBlankLines();

        Func<int, int, int> slowAdd = (a, b) =>
        {
            Console.WriteLine($"    [계산 중] {a} + {b} 계산...");
            return a + b;
        };

        // 2인자 함수의 수동 캐싱
        var addCache = new Dictionary<(int, int), int>();
        Func<int, int, int> memoAdd = (a, b) =>
        {
            var key = (a, b);
            if (addCache.TryGetValue(key, out var cached)) return cached;
            var result = slowAdd(a, b);
            addCache[key] = result;
            return result;
        };

        MenuHelper.PrintCode("// 2인자 함수 메모이제이션");
        Console.WriteLine($"  memoAdd(3, 5) = {memoAdd(3, 5)}");
        Console.WriteLine($"  memoAdd(3, 5) = {memoAdd(3, 5)}");  // 캐시됨
        Console.WriteLine($"  memoAdd(3, 7) = {memoAdd(3, 7)}");  // 새로운 인자 조합

        // ============================================================
        // 3. 피보나치 수열 (재귀 + 메모이제이션)
        // ============================================================
        MenuHelper.PrintSubHeader("3. 피보나치 수열 (재귀)");

        MenuHelper.PrintExplanation("재귀 함수에서 메모이제이션의 위력을 확인합니다.");
        MenuHelper.PrintExplanation("피보나치는 동일한 값을 여러 번 계산하는 대표적인 예입니다.");
        MenuHelper.PrintBlankLines();

        // 메모이제이션 없이
        MenuHelper.PrintCode("// 메모이제이션 없이 (지수 시간)");
        var startTime = DateTime.Now;
        var fib30 = FibNaive(30);
        var naiveTime = (DateTime.Now - startTime).TotalMilliseconds;
        MenuHelper.PrintResult($"FibNaive(30)", $"{fib30} (소요: {naiveTime:F0}ms)");
        MenuHelper.PrintBlankLines();

        // 메모이제이션 사용
        MenuHelper.PrintCode("// 메모이제이션 적용 (선형 시간)");
        var cache = new Dictionary<int, long>();
        startTime = DateTime.Now;
        var fib30Memo = FibMemoized(30, cache);
        var memoTime = (DateTime.Now - startTime).TotalMilliseconds;
        MenuHelper.PrintResult($"FibMemoized(30)", $"{fib30Memo} (소요: {memoTime:F0}ms)");

        // ============================================================
        // 4. 메모이제이션 주의사항
        // ============================================================
        MenuHelper.PrintSubHeader("4. 메모이제이션 주의사항");

        MenuHelper.PrintExplanation("1. 순수 함수에만 사용: 부수 효과가 있으면 캐시가 잘못될 수 있음");
        MenuHelper.PrintExplanation("2. 메모리 사용: 모든 결과가 캐시되므로 메모리 주의");
        MenuHelper.PrintExplanation("3. 참조 동등성: 인자가 참조 타입이면 주의 필요");
        MenuHelper.PrintBlankLines();

        // 잘못된 사용 예 (부수 효과 있음)
        MenuHelper.PrintWarning("잘못된 예: 부수 효과가 있는 함수");
        int counter = 0;
        Func<int, int> impureFunc = x =>
        {
            counter++;  // 부수 효과!
            return x * counter;
        };

        // 부수 효과가 있는 함수의 메모이제이션 (문제 시연용)
        var impureCache = new Dictionary<int, int>();
        Func<int, int> memoImpure = x =>
        {
            if (impureCache.TryGetValue(x, out var cached)) return cached;
            var result = impureFunc(x);
            impureCache[x] = result;
            return result;
        };
        Console.WriteLine($"  memoImpure(5) = {memoImpure(5)} (counter={counter})");
        Console.WriteLine($"  memoImpure(5) = {memoImpure(5)} (counter={counter})");  // 캐시됨
        Console.WriteLine($"  memoImpure(3) = {memoImpure(3)} (counter={counter})");  // 새로 계산
        MenuHelper.PrintExplanation("counter가 증가하지 않아 결과가 예상과 다를 수 있음");

        // ============================================================
        // 5. 실전 예제: API 응답 캐싱
        // ============================================================
        MenuHelper.PrintSubHeader("5. 실전 예제: API 응답 캐싱");

        MenuHelper.PrintExplanation("비용이 큰 연산(API 호출 등)을 캐싱하는 패턴입니다.");
        MenuHelper.PrintBlankLines();

        // API 호출 시뮬레이션
        Func<string, string> fetchUserData = userId =>
        {
            Console.WriteLine($"    [API 호출] 사용자 {userId} 데이터 조회 중...");
            Thread.Sleep(200);  // API 지연 시뮬레이션
            return $"User({userId}): 데이터 로드됨";
        };

        var userCache = new Dictionary<string, string>();
        Func<string, string> memoFetchUser = userId =>
        {
            if (userCache.TryGetValue(userId, out var cached)) return cached;
            var result = fetchUserData(userId);
            userCache[userId] = result;
            return result;
        };

        MenuHelper.PrintCode("// 첫 번째 호출 (API 호출됨)");
        Console.WriteLine($"  {memoFetchUser("user_123")}");

        MenuHelper.PrintCode("// 두 번째 호출 (캐시에서)");
        Console.WriteLine($"  {memoFetchUser("user_123")}");

        MenuHelper.PrintCode("// 다른 사용자 (새로 호출)");
        Console.WriteLine($"  {memoFetchUser("user_456")}");

        // ============================================================
        // 6. 문자열 처리 캐싱
        // ============================================================
        MenuHelper.PrintSubHeader("6. 문자열 처리 캐싱");

        MenuHelper.PrintExplanation("비용이 큰 문자열 처리도 메모이제이션 대상입니다.");
        MenuHelper.PrintBlankLines();

        Func<string, int> countWords = text =>
        {
            Console.WriteLine($"    [처리 중] 단어 수 계산...");
            return text.Split(' ', StringSplitOptions.RemoveEmptyEntries).Length;
        };

        var wordCountCache = new Dictionary<string, int>();
        Func<string, int> memoCountWords = text =>
        {
            if (wordCountCache.TryGetValue(text, out var cached)) return cached;
            var result = countWords(text);
            wordCountCache[text] = result;
            return result;
        };

        var sampleText = "Hello world this is a sample text for testing";
        Console.WriteLine($"  첫 번째: {memoCountWords(sampleText)} 단어");
        Console.WriteLine($"  두 번째: {memoCountWords(sampleText)} 단어");
        Console.WriteLine($"  세 번째: {memoCountWords(sampleText)} 단어");

        // ============================================================
        // 7. 조건부 메모이제이션 패턴
        // ============================================================
        MenuHelper.PrintSubHeader("7. 조건부 캐싱 패턴");

        MenuHelper.PrintExplanation("특정 조건에서만 캐싱하는 패턴입니다.");
        MenuHelper.PrintBlankLines();

        var expensiveCache = new Dictionary<int, int>();

        Func<int, int> conditionalMemo = n =>
        {
            // 양수만 캐싱
            if (n > 0 && expensiveCache.TryGetValue(n, out var cached))
            {
                Console.WriteLine($"    [캐시 히트] n={n}");
                return cached;
            }

            Console.WriteLine($"    [계산] n={n}");
            var result = n * n;

            if (n > 0)
            {
                expensiveCache[n] = result;
            }

            return result;
        };

        Console.WriteLine($"  conditionalMemo(5) = {conditionalMemo(5)}");
        Console.WriteLine($"  conditionalMemo(5) = {conditionalMemo(5)}");
        Console.WriteLine($"  conditionalMemo(-3) = {conditionalMemo(-3)}");
        Console.WriteLine($"  conditionalMemo(-3) = {conditionalMemo(-3)}");

        MenuHelper.PrintSuccess("메모이제이션 학습 완료!");
    }

    // 순수 재귀 피보나치 (비효율적)
    private static long FibNaive(int n)
    {
        if (n <= 1) return n;
        return FibNaive(n - 1) + FibNaive(n - 2);
    }

    // 메모이제이션된 피보나치
    private static long FibMemoized(int n, Dictionary<int, long> cache)
    {
        if (n <= 1) return n;

        if (cache.TryGetValue(n, out var cached))
            return cached;

        var result = FibMemoized(n - 1, cache) + FibMemoized(n - 2, cache);
        cache[n] = result;
        return result;
    }
}
