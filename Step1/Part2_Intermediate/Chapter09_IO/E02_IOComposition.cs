using LanguageExt;
using LanguageExt.Common;
using LanguageExt101.Common;
using static LanguageExt.Prelude;

namespace LanguageExt101.Part2_Intermediate.Chapter09_IO;

/// <summary>
/// IO 합성(Composition)을 학습합니다.
///
/// 학습 목표:
/// - Map을 사용한 값 변환
/// - Bind를 사용한 IO 연결
/// - LINQ 쿼리 구문으로 복잡한 파이프라인 구성
/// - 합성의 장점 (재사용성, 가독성)
///
/// 핵심 개념:
/// IO 합성은 작은 IO 연산들을 조합하여 복잡한 연산을 만드는 것입니다.
/// Map은 값을 변환하고, Bind는 IO를 반환하는 함수를 연결합니다.
/// LINQ 구문을 사용하면 더 읽기 쉬운 코드를 작성할 수 있습니다.
/// </summary>
public static class E02_IOComposition
{
    public static void Run()
    {
        MenuHelper.PrintHeader("Chapter 09-E02: IO 합성");

        // ============================================================
        // 1. Map 기초 - 단일 값 변환
        // ============================================================
        MenuHelper.PrintSubHeader("1. Map 기초 - 단일 값 변환");

        MenuHelper.PrintExplanation("Map은 IO 안의 값을 변환합니다.");
        MenuHelper.PrintExplanation("IO<A>.Map(f) => IO<B> where f: A -> B");
        MenuHelper.PrintBlankLines();

        var ioNumber = IO.pure(10);

        MenuHelper.PrintCode("var doubled = IO.pure(10).Map(x => x * 2);");
        var doubled = ioNumber.Map(x => x * 2);
        MenuHelper.PrintResult("doubled.Run()", doubled.Run());

        MenuHelper.PrintCode("var asString = IO.pure(10).Map(x => $\"값: {x}\");");
        var asString = ioNumber.Map(x => $"값: {x}");
        MenuHelper.PrintResult("asString.Run()", asString.Run());

        // ============================================================
        // 2. Map 체이닝 - 여러 변환 연결
        // ============================================================
        MenuHelper.PrintSubHeader("2. Map 체이닝 - 여러 변환 연결");

        MenuHelper.PrintExplanation("여러 Map을 연결하여 복잡한 변환을 구성합니다.");
        MenuHelper.PrintBlankLines();

        MenuHelper.PrintCode(@"var chained = IO.pure(5)
    .Map(x => x * 2)      // 10
    .Map(x => x + 3)      // 13
    .Map(x => x * x)      // 169
    .Map(x => $""결과: {x}"");");

        var chained = IO.pure(5)
            .Map(x => x * 2)
            .Map(x => x + 3)
            .Map(x => x * x)
            .Map(x => $"결과: {x}");

        MenuHelper.PrintResult("chained.Run()", chained.Run());

        // 실제 예시: 문자열 처리
        MenuHelper.PrintCode(@"var processed = IO.pure(""  Hello World  "")
    .Map(s => s.Trim())
    .Map(s => s.ToLower())
    .Map(s => s.Replace("" "", ""-""));");

        var processed = IO.pure("  Hello World  ")
            .Map(s => s.Trim())
            .Map(s => s.ToLower())
            .Map(s => s.Replace(" ", "-"));

        MenuHelper.PrintResult("processed.Run()", processed.Run());

        // ============================================================
        // 3. Bind 기초 - IO 반환 함수 연결
        // ============================================================
        MenuHelper.PrintSubHeader("3. Bind 기초 - IO 반환 함수 연결");

        MenuHelper.PrintExplanation("Bind는 IO를 반환하는 함수를 연결합니다.");
        MenuHelper.PrintExplanation("IO<A>.Bind(f) => IO<B> where f: A -> IO<B>");
        MenuHelper.PrintBlankLines();

        IO<int> ParseInt(string s) =>
            int.TryParse(s, out var n)
                ? IO.pure(n)
                : IO.fail<int>(Error.New("파싱 실패"));

        IO<int> Double(int n) => IO.pure(n * 2);

        MenuHelper.PrintCode(@"var bound = IO.pure(""42"")
    .Bind(ParseInt)
    .Bind(Double);");

        var bound = IO.pure("42")
            .Bind(ParseInt)
            .Bind(Double);

        MenuHelper.PrintResult("bound.Run()", bound.Run());

        // 부수 효과가 있는 Bind
        MenuHelper.PrintCode(@"var withSideEffect = IO.pure(100)
    .Bind(n => IO.lift(() => { Console.WriteLine($""    처리 중: {n}""); return n * 2; }));");

        var withSideEffect = IO.pure(100)
            .Bind(n => IO.lift(() =>
            {
                Console.WriteLine($"    처리 중: {n}");
                return n * 2;
            }));

        MenuHelper.PrintResult("withSideEffect.Run()", withSideEffect.Run());

        // ============================================================
        // 4. Bind vs Map 비교
        // ============================================================
        MenuHelper.PrintSubHeader("4. Bind vs Map 비교");

        MenuHelper.PrintExplanation("Map: A -> B (순수 함수)");
        MenuHelper.PrintExplanation("Bind: A -> IO<B> (IO 반환 함수)");
        MenuHelper.PrintBlankLines();

        // Map은 순수 변환
        MenuHelper.PrintCode("// Map: 순수 변환 (A -> B)");
        var mapExample = IO.pure(5).Map(x => x * 2);
        MenuHelper.PrintResult("Map 결과", mapExample.Run());

        // Bind는 IO 반환
        MenuHelper.PrintCode("// Bind: IO 반환 함수 (A -> IO<B>)");
        var bindExample = IO.pure(5).Bind(x => IO.pure(x * 2));
        MenuHelper.PrintResult("Bind 결과", bindExample.Run());

        // Map으로 IO를 반환하면 IO<IO<A>>가 됨 (원하지 않는 결과)
        MenuHelper.PrintExplanation("Map으로 IO 반환 함수를 사용하면 IO<IO<A>>가 됩니다.");
        MenuHelper.PrintExplanation("Bind는 이를 자동으로 평탄화(flatten)합니다.");

        // ============================================================
        // 5. LINQ 쿼리 구문
        // ============================================================
        MenuHelper.PrintSubHeader("5. LINQ 쿼리 구문");

        MenuHelper.PrintExplanation("LINQ from-select 구문으로 더 읽기 쉬운 코드를 작성합니다.");
        MenuHelper.PrintBlankLines();

        MenuHelper.PrintCode(@"var linqResult =
    from a in IO.pure(10)
    from b in IO.pure(20)
    select a + b;");

        var linqResult =
            from a in IO.pure(10)
            from b in IO.pure(20)
            select a + b;

        MenuHelper.PrintResult("linqResult.Run()", linqResult.Run());

        // let 절 사용 (중간 값 바인딩)
        MenuHelper.PrintCode(@"var withLet =
    from x in IO.pure(42)
    let multiplied = x * 2
    select $""원본: {x}, 두배: {multiplied}"";");

        var withLet =
            from x in IO.pure(42)
            let multiplied = x * 2
            select $"원본: {x}, 두배: {multiplied}";

        MenuHelper.PrintResult("withLet.Run()", withLet.Run());

        // ============================================================
        // 6. 복잡한 파이프라인 - 여러 IO 순차 조합
        // ============================================================
        MenuHelper.PrintSubHeader("6. 복잡한 파이프라인");

        MenuHelper.PrintExplanation("여러 IO 연산을 LINQ로 순차적으로 조합합니다.");
        MenuHelper.PrintBlankLines();

        IO<string> GetUserId() => IO.lift(() =>
        {
            Console.WriteLine("    [1] 사용자 ID 조회 중...");
            return "user-123";
        });

        IO<string> GetUserName(string userId) => IO.lift(() =>
        {
            Console.WriteLine($"    [2] 사용자 이름 조회 중... (ID: {userId})");
            return "홍길동";
        });

        IO<int> GetUserAge(string userId) => IO.lift(() =>
        {
            Console.WriteLine($"    [3] 사용자 나이 조회 중... (ID: {userId})");
            return 30;
        });

        IO<string> FormatProfile(string name, int age) => IO.pure($"{name} ({age}세)");

        var pipeline =
            from userId in GetUserId()
            from name in GetUserName(userId)
            from age in GetUserAge(userId)
            from profile in FormatProfile(name, age)
            select profile;

        MenuHelper.PrintResult("프로필", pipeline.Run());

        // ============================================================
        // 7. 재사용 가능한 IO 조합
        // ============================================================
        MenuHelper.PrintSubHeader("7. 재사용 가능한 IO 조합");

        MenuHelper.PrintExplanation("공통 패턴을 함수로 추출하여 재사용합니다.");
        MenuHelper.PrintBlankLines();

        // 로깅 래퍼
        IO<T> WithLogging<T>(string operation, IO<T> io) =>
            from _ in IO.lift(() => { Console.WriteLine($"    [시작] {operation}"); return unit; })
            from result in io
            from __ in IO.lift(() => { Console.WriteLine($"    [완료] {operation}: {result}"); return unit; })
            select result;

        // 타이밍 래퍼
        IO<(T result, long elapsedMs)> WithTiming<T>(IO<T> io) =>
            from start in IO.lift(() => DateTime.Now.Ticks)
            from result in io
            from end in IO.lift(() => DateTime.Now.Ticks)
            let elapsed = (end - start) / TimeSpan.TicksPerMillisecond
            select (result, elapsed);

        var simpleOp = IO.lift(() =>
        {
            Thread.Sleep(100);
            return 42;
        });

        MenuHelper.PrintCode("WithLogging(\"계산\", simpleOp)");
        var logged = WithLogging("계산", simpleOp);
        logged.Run();
        MenuHelper.PrintBlankLines();

        MenuHelper.PrintCode("WithTiming(simpleOp)");
        var timed = WithTiming(simpleOp);
        var (timedResult, elapsed) = timed.Run();
        MenuHelper.PrintResult("결과", timedResult);
        MenuHelper.PrintResult("소요 시간", $"{elapsed}ms");

        // ============================================================
        // 8. 실전 예제: 데이터 처리 파이프라인
        // ============================================================
        MenuHelper.PrintSubHeader("8. 실전 예제: 데이터 처리 파이프라인");

        MenuHelper.PrintExplanation("CSV 데이터를 로드 -> 파싱 -> 필터 -> 집계하는 파이프라인");
        MenuHelper.PrintBlankLines();

        // 시뮬레이션된 CSV 데이터
        var csvData = "이름,나이,부서\n홍길동,30,개발\n김철수,25,디자인\n이영희,35,개발\n박민수,28,마케팅";

        IO<string> LoadCsv(string source) => IO.lift(() =>
        {
            Console.WriteLine($"    CSV 로드 중: {source}");
            return csvData;
        });

        IO<Seq<(string name, int age, string dept)>> ParseCsv(string csv) => IO.lift(() =>
        {
            Console.WriteLine("    CSV 파싱 중...");
            var lines = csv.Split('\n').Skip(1); // 헤더 제외
            return toSeq(lines.Select(line =>
            {
                var parts = line.Split(',');
                return (parts[0], int.Parse(parts[1]), parts[2]);
            }));
        });

        IO<Seq<(string name, int age, string dept)>> FilterByDept(
            Seq<(string name, int age, string dept)> records,
            string dept) => IO.lift(() =>
        {
            Console.WriteLine($"    {dept} 부서 필터링 중...");
            return toSeq(records.Where(r => r.dept == dept));
        });

        IO<double> CalculateAverageAge(Seq<(string name, int age, string dept)> records) =>
            IO.lift(() =>
            {
                Console.WriteLine("    평균 나이 계산 중...");
                return records.Select(r => (double)r.age).ToArray().Average();
            });

        var dataPipeline =
            from csv in LoadCsv("employees.csv")
            from records in ParseCsv(csv)
            from devRecords in FilterByDept(records, "개발")
            from avgAge in CalculateAverageAge(devRecords)
            select new
            {
                Department = "개발",
                EmployeeCount = devRecords.Count,
                AverageAge = avgAge
            };

        var stats = dataPipeline.Run();
        MenuHelper.PrintBlankLines();
        MenuHelper.PrintResult("부서", stats.Department);
        MenuHelper.PrintResult("직원 수", stats.EmployeeCount);
        MenuHelper.PrintResult("평균 나이", $"{stats.AverageAge:F1}세");

        MenuHelper.PrintSuccess("IO 합성 학습 완료!");
    }
}
