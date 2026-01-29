using LanguageExt;
using LanguageExt.Common;
using LanguageExt101.Common;
using static LanguageExt.Prelude;

namespace LanguageExt101.Part2_Intermediate.Chapter09_IO;

/// <summary>
/// IO를 사용한 파일 작업 실습입니다.
///
/// 학습 목표:
/// - 파일 읽기/쓰기 IO 래핑
/// - 여러 파일 처리 (순차/병렬)
/// - 에러 처리와 폴백
/// - 실전적인 파일 작업 패턴
///
/// 핵심 개념:
/// 파일 I/O는 대표적인 부수 효과입니다.
/// IO 모나드로 래핑하면 순수 함수형 스타일로 파일 작업을 합성할 수 있습니다.
/// </summary>
public static class Exercise06_FileOperations
{
    // 가상 파일 시스템 (실제 파일 대신 메모리 사용)
    private static readonly Dictionary<string, string> _virtualFiles = new()
    {
        ["config.json"] = "{ \"name\": \"app\", \"version\": \"1.0\" }",
        ["data.txt"] = "Line 1\nLine 2\nLine 3",
        ["log1.txt"] = "[INFO] Started\n[ERROR] Failed\n[INFO] Retrying",
        ["log2.txt"] = "[WARN] Low memory\n[INFO] Completed",
        ["users.csv"] = "id,name,age\n1,홍길동,30\n2,김철수,25\n3,이영희,35",
    };

    public static void Run()
    {
        MenuHelper.PrintHeader("Exercise 06: 파일 작업");

        // ============================================================
        // 1. 파일 읽기 IO
        // ============================================================
        MenuHelper.PrintSubHeader("1. 파일 읽기 IO");

        MenuHelper.PrintExplanation("파일 읽기를 IO로 래핑하여 부수 효과를 캡슐화합니다.");
        MenuHelper.PrintBlankLines();

        IO<string> ReadFile(string path) => IO.lift(() =>
        {
            Console.WriteLine($"    [읽기] {path}");
            if (!_virtualFiles.TryGetValue(path, out var content))
                throw new FileNotFoundException($"파일을 찾을 수 없습니다: {path}");
            return content;
        });

        MenuHelper.PrintCode(@"IO<string> ReadFile(string path) => IO.lift(() => {
    if (!_virtualFiles.TryGetValue(path, out var content))
        throw new FileNotFoundException(...);
    return content;
});");

        var readConfig = ReadFile("config.json");
        MenuHelper.PrintResult("config.json", readConfig.Run());

        var readData = ReadFile("data.txt");
        MenuHelper.PrintResult("data.txt", readData.Run().Replace("\n", "\\n"));

        // ============================================================
        // 2. 파일 쓰기 IO
        // ============================================================
        MenuHelper.PrintSubHeader("2. 파일 쓰기 IO");

        MenuHelper.PrintExplanation("파일 쓰기도 IO로 래핑합니다.");
        MenuHelper.PrintBlankLines();

        IO<Unit> WriteFile(string path, string content) => IO.lift(() =>
        {
            Console.WriteLine($"    [쓰기] {path} ({content.Length} bytes)");
            _virtualFiles[path] = content;
            return unit;
        });

        IO<Unit> AppendFile(string path, string content) => IO.lift(() =>
        {
            Console.WriteLine($"    [추가] {path} (+{content.Length} bytes)");
            if (_virtualFiles.TryGetValue(path, out var existing))
                _virtualFiles[path] = existing + content;
            else
                _virtualFiles[path] = content;
            return unit;
        });

        MenuHelper.PrintCode(@"var writeOp = WriteFile(""output.txt"", ""Hello World!"");
writeOp.Run();");

        var writeOp = WriteFile("output.txt", "Hello World!");
        writeOp.Run();

        var verifyWrite = ReadFile("output.txt");
        MenuHelper.PrintResult("output.txt (검증)", verifyWrite.Run());

        // ============================================================
        // 3. 파일 존재 확인과 Option
        // ============================================================
        MenuHelper.PrintSubHeader("3. 파일 존재 확인과 Option");

        MenuHelper.PrintExplanation("파일이 없을 수 있는 경우 Option<string>을 반환합니다.");
        MenuHelper.PrintBlankLines();

        IO<bool> FileExists(string path) => IO.lift(() =>
        {
            var exists = _virtualFiles.ContainsKey(path);
            Console.WriteLine($"    [존재 확인] {path}: {(exists ? "있음" : "없음")}");
            return exists;
        });

        IO<Option<string>> TryReadFile(string path) => IO.lift(() =>
        {
            Console.WriteLine($"    [안전 읽기] {path}");
            return _virtualFiles.TryGetValue(path, out var content)
                ? Some(content)
                : None;
        });

        MenuHelper.PrintCode(@"var maybeContent = TryReadFile(""nonexistent.txt"").Run();
maybeContent.Match(
    Some: c => Console.WriteLine(c),
    None: () => Console.WriteLine(""파일 없음"")
);");

        var maybeContent1 = TryReadFile("config.json").Run();
        maybeContent1.Match(
            Some: c => MenuHelper.PrintResult("config.json 내용", c),
            None: () => MenuHelper.PrintResult("config.json", "파일 없음")
        );

        var maybeContent2 = TryReadFile("nonexistent.txt").Run();
        maybeContent2.Match(
            Some: c => MenuHelper.PrintResult("nonexistent.txt 내용", c),
            None: () => MenuHelper.PrintResult("nonexistent.txt", "파일 없음")
        );

        // ============================================================
        // 4. 순차 처리 - 여러 파일 순서대로 읽기
        // ============================================================
        MenuHelper.PrintSubHeader("4. 순차 처리 - 여러 파일 순서대로 읽기");

        MenuHelper.PrintExplanation("Sequence로 여러 IO를 순차적으로 실행합니다.");
        MenuHelper.PrintBlankLines();

        var filesToRead = Seq("config.json", "data.txt", "users.csv");

        MenuHelper.PrintCode(@"var files = Seq(""config.json"", ""data.txt"", ""users.csv"");
// LINQ를 사용하여 순차적으로 읽기
var readAll = files.Aggregate(
    IO.pure(Seq<string>()),
    (acc, file) => from list in acc
                   from content in ReadFile(file)
                   select list.Add(content));");

        var readAllSeq = filesToRead.Aggregate(
            IO.pure(Seq<string>()),
            (acc, file) => from list in acc
                           from content in ReadFile(file)
                           select list.Add(content));
        var contentsSeq = readAllSeq.Run();

        Console.WriteLine("  순차 읽기 결과:");
        var idx = 0;
        foreach (var (file, content) in filesToRead.Zip(contentsSeq))
        {
            var preview = content.Length > 30 ? content[..30] + "..." : content;
            Console.WriteLine($"    [{++idx}] {file}: {preview.Replace("\n", "\\n")}");
        }

        // ============================================================
        // 5. 병렬 처리 - 여러 파일 동시 읽기
        // ============================================================
        MenuHelper.PrintSubHeader("5. 병렬 처리 - 여러 파일 동시 읽기");

        MenuHelper.PrintExplanation("awaitAll로 여러 파일을 동시에 읽습니다.");
        MenuHelper.PrintBlankLines();

        IO<string> ReadFileWithDelay(string path, int delayMs) => IO.lift(() =>
        {
            Console.WriteLine($"    [{DateTime.Now:HH:mm:ss.fff}] {path} 읽기 시작...");
            Thread.Sleep(delayMs);
            var content = _virtualFiles.GetValueOrDefault(path, "");
            Console.WriteLine($"    [{DateTime.Now:HH:mm:ss.fff}] {path} 읽기 완료");
            return content;
        });

        var startTime = DateTime.Now;
        var parallelRead = awaitAll(
            ReadFileWithDelay("config.json", 200),
            ReadFileWithDelay("data.txt", 200),
            ReadFileWithDelay("users.csv", 200)
        );

        var parallelResults = parallelRead.Run();
        var elapsed = (DateTime.Now - startTime).TotalMilliseconds;
        MenuHelper.PrintResult("병렬 읽기 시간", $"{elapsed:F0}ms (순차 시 ~600ms 예상)");

        // ============================================================
        // 6. 에러 처리와 폴백
        // ============================================================
        MenuHelper.PrintSubHeader("6. 에러 처리와 폴백");

        MenuHelper.PrintExplanation("파일을 읽을 수 없을 때 기본값을 제공합니다.");
        MenuHelper.PrintBlankLines();

        IO<string> ReadFileWithFallback(string path, string fallback) =>
            ReadFile(path)
                .Catch(_ => IO.pure(fallback))
                .As();

        MenuHelper.PrintCode(@"IO<string> ReadFileWithFallback(string path, string fallback) =>
    ReadFile(path).Catch(_ => IO.pure(fallback));");

        var withFallback1 = ReadFileWithFallback("config.json", "{}").Run();
        MenuHelper.PrintResult("config.json (폴백)", withFallback1);

        var withFallback2 = ReadFileWithFallback("missing.json", "{}").Run();
        MenuHelper.PrintResult("missing.json (폴백)", withFallback2);

        // 여러 소스에서 시도
        MenuHelper.PrintBlankLines();
        MenuHelper.PrintExplanation("여러 소스 중 첫 번째 성공을 사용합니다:");

        IO<string> ReadFromSources(params string[] sources)
        {
            return sources.Aggregate(
                IO.fail<string>(Error.New("모든 소스 실패")),
                (acc, source) => acc | ReadFile(source)
            );
        }

        MenuHelper.PrintCode(@"var fromMultiple = ReadFromSources(""missing1.json"", ""missing2.json"", ""config.json"");");
        var fromMultiple = ReadFromSources("missing1.json", "missing2.json", "config.json").Run();
        MenuHelper.PrintResult("다중 소스 결과", fromMultiple);

        // ============================================================
        // 7. 파일 복사/이동 - IO 합성
        // ============================================================
        MenuHelper.PrintSubHeader("7. 파일 복사/이동 - IO 합성");

        MenuHelper.PrintExplanation("읽기와 쓰기를 합성하여 복사/이동을 구현합니다.");
        MenuHelper.PrintBlankLines();

        IO<Unit> CopyFile(string source, string dest) =>
            from content in ReadFile(source)
            from _ in WriteFile(dest, content)
            from __ in IO.lift(() =>
            {
                Console.WriteLine($"    [복사 완료] {source} -> {dest}");
                return unit;
            })
            select unit;

        IO<Unit> DeleteFile(string path) => IO.lift(() =>
        {
            if (_virtualFiles.Remove(path))
                Console.WriteLine($"    [삭제] {path}");
            else
                Console.WriteLine($"    [삭제 실패] {path} (파일 없음)");
            return unit;
        });

        IO<Unit> MoveFile(string source, string dest) =>
            from _ in CopyFile(source, dest)
            from __ in DeleteFile(source)
            from ___ in IO.lift(() =>
            {
                Console.WriteLine($"    [이동 완료] {source} -> {dest}");
                return unit;
            })
            select unit;

        MenuHelper.PrintCode(@"var copyOp = CopyFile(""config.json"", ""config.backup.json"");
copyOp.Run();");

        var copyOp = CopyFile("config.json", "config.backup.json");
        copyOp.Run();

        MenuHelper.PrintBlankLines();
        MenuHelper.PrintCode(@"var moveOp = MoveFile(""output.txt"", ""archived/output.txt"");
moveOp.Run();");

        var moveOp = MoveFile("output.txt", "archived/output.txt");
        moveOp.Run();

        // ============================================================
        // 8. 실전 예제: 로그 파일 분석
        // ============================================================
        MenuHelper.PrintSubHeader("8. 실전 예제: 로그 파일 분석");

        MenuHelper.PrintExplanation("여러 로그 파일을 읽어 레벨별로 집계합니다.");
        MenuHelper.PrintBlankLines();

        IO<Seq<string>> ReadLogLines(string path) =>
            from content in ReadFile(path)
            select toSeq(content.Split('\n', StringSplitOptions.RemoveEmptyEntries));

        IO<Map<string, int>> CountLogLevels(Seq<string> lines) => IO.lift(() =>
        {
            var counts = new Dictionary<string, int>
            {
                ["INFO"] = 0,
                ["WARN"] = 0,
                ["ERROR"] = 0,
            };

            foreach (var line in lines)
            {
                if (line.Contains("[INFO]")) counts["INFO"]++;
                else if (line.Contains("[WARN]")) counts["WARN"]++;
                else if (line.Contains("[ERROR]")) counts["ERROR"]++;
            }

            return toMap(counts.Select(kv => (kv.Key, kv.Value)));
        });

        IO<Map<string, int>> MergeCounts(Map<string, int> a, Map<string, int> b) => IO.pure(
            toMap(
                from key in a.Keys.Concat(b.Keys).Distinct()
                let countA = a.Find(key).IfNone(0)
                let countB = b.Find(key).IfNone(0)
                select (key, countA + countB)
            )
        );

        var logAnalysis =
            from log1Lines in ReadLogLines("log1.txt")
            from log2Lines in ReadLogLines("log2.txt")
            let allLines = log1Lines.Concat(log2Lines)
            from counts in CountLogLevels(toSeq(allLines))
            select counts;

        MenuHelper.PrintCode(@"var logAnalysis =
    from log1Lines in ReadLogLines(""log1.txt"")
    from log2Lines in ReadLogLines(""log2.txt"")
    let allLines = log1Lines.Concat(log2Lines)
    from counts in CountLogLevels(allLines)
    select counts;");

        MenuHelper.PrintBlankLines();
        Console.WriteLine("  로그 분석 중...");
        var logCounts = logAnalysis.Run();

        MenuHelper.PrintBlankLines();
        Console.WriteLine("  === 로그 레벨 분석 결과 ===");
        foreach (var (level, count) in logCounts)
        {
            var bar = new string('#', count * 5);
            Console.WriteLine($"    {level,-6}: {count} {bar}");
        }

        // 보너스: 병렬로 여러 로그 파일 분석
        MenuHelper.PrintBlankLines();
        MenuHelper.PrintExplanation("보너스: 병렬 로그 분석");

        var parallelLogAnalysis =
            from results in awaitAll(
                ReadLogLines("log1.txt").Bind(CountLogLevels),
                ReadLogLines("log2.txt").Bind(CountLogLevels)
            )
            from merged in results.Aggregate(
                IO.pure(Map<string, int>()),
                (acc, cur) => acc.Bind(a => MergeCounts(a, cur))
            )
            select merged;

        var parallelCounts = parallelLogAnalysis.Run();
        Console.WriteLine("  병렬 분석 결과:");
        foreach (var (level, count) in parallelCounts)
        {
            Console.WriteLine($"    {level}: {count}건");
        }

        MenuHelper.PrintSuccess("실습 06 완료!");
    }
}
