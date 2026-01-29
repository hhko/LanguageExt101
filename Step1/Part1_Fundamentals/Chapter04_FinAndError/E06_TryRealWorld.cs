using System.Text.Json;
using LanguageExt;
using LanguageExt.Common;
using LanguageExt101.Common;
using static LanguageExt.Prelude;

namespace LanguageExt101.Part1_Fundamentals.Chapter04_FinAndError;

/// <summary>
/// Try의 실무 활용 패턴을 학습합니다.
/// 파일 I/O, JSON 파싱, 재시도 패턴, 타입 변환, | 연산자와 @catch 등
/// </summary>
public static class E06_TryRealWorld
{
    public static void Run()
    {
        MenuHelper.PrintHeader("Chapter 04-E06: 실무 Try 활용");

        // ============================================================
        // 1. 기존 try-catch vs Try 비교
        // ============================================================
        MenuHelper.PrintSubHeader("1. 기존 try-catch vs Try 비교");

        MenuHelper.PrintCode(@"// 전통적 방식 - 중첩된 try-catch
string? LoadConfig_Traditional()
{
    try
    {
        var json = File.ReadAllText(""config.json"");
        try
        {
            var config = JsonSerializer.Deserialize<Config>(json);
            try
            {
                ValidateConfig(config);
                return config.Name;
            }
            catch (ValidationException) { return null; }
        }
        catch (JsonException) { return null; }
    }
    catch (IOException) { return null; }
}");

        MenuHelper.PrintBlankLines();

        MenuHelper.PrintCode(@"// Try 방식 - 평탄한 파이프라인
Try<string> LoadConfig_Try() =>
    from json   in Try.lift(() => File.ReadAllText(""config.json""))
    from config in Try.lift(() => JsonSerializer.Deserialize<Config>(json))
    from valid  in ValidateConfig(config)
    select valid.Name;");

        MenuHelper.PrintExplanation("Try를 사용하면 에러 처리가 파이프라인 끝에서 한 번만 이루어집니다.");

        // ============================================================
        // 2. JSON 파싱 파이프라인
        // ============================================================
        MenuHelper.PrintSubHeader("2. JSON 파싱 파이프라인");

        // 성공 케이스
        var validJson = """{"Name": "MyApp", "Version": 1, "Debug": true}""";
        MenuHelper.PrintCode($@"var validJson = ""{validJson}"";
var parsed = ParseConfig(validJson);");

        var parsed = ParseConfig(validJson);
        MenuHelper.PrintResult("ParseConfig(validJson).Run()", parsed.Run());

        // 실패 케이스
        var invalidJson = """{"Name": "MyApp", "Version":}""";
        MenuHelper.PrintCode($@"var invalidJson = ""{invalidJson}"";
var parsedInvalid = ParseConfig(invalidJson);");

        var parsedInvalid = ParseConfig(invalidJson);
        MenuHelper.PrintResult("ParseConfig(invalidJson).Run()", parsedInvalid.Run());

        // ============================================================
        // 3. | 연산자로 폴백 체인 구현
        // ============================================================
        MenuHelper.PrintSubHeader("3. | 연산자로 폴백 체인 구현");

        MenuHelper.PrintExplanation("| 연산자는 Choice 트레이트의 Choose를 호출합니다.");
        MenuHelper.PrintExplanation("첫 번째가 실패하면 두 번째를 시도 (단락 평가).");
        MenuHelper.PrintBlankLines();

        MenuHelper.PrintCode(@"// 설정 파일 폴백 체인
var config =
    LoadAppConfig(""config.prod.json"")   // 1. 프로덕션 설정 시도
    | LoadAppConfig(""config.json"")      // 2. 기본 설정 시도
    | Try.Succ(AppConfig.Default);        // 3. 하드코딩 기본값");

        Console.WriteLine("  [폴백 체인 실행]");
        var config =
            LoadAppConfig("config.prod.json")
            | LoadAppConfig("config.json")
            | Try.Succ(AppConfig.Default);

        MenuHelper.PrintResult("config.Run()", config.Run());

        // ============================================================
        // 4. | 연산자와 @catch 조합
        // ============================================================
        MenuHelper.PrintSubHeader("4. | 연산자와 @catch 조합");

        MenuHelper.PrintExplanation("| 연산자는 CatchM, Error, Exception과도 조합됩니다.");
        MenuHelper.PrintExplanation("파이프라인에서 유연한 에러 처리가 가능합니다.");
        MenuHelper.PrintBlankLines();

        // | @catch - 특정 조건에서만 복구
        MenuHelper.PrintCode(@"// @catch로 특정 에러만 처리
var withCatch = Try.lift(() => int.Parse(""abc""))
    | @catch<Try, int>(
        e => e.Message.Contains(""Format""),  // FormatException만
        e => Try.Succ(-1)
    );");

        var withCatch = Try.lift(() => int.Parse("abc"))
            | @catch<Try, int>(
                e => e.Message.Contains("Format"),
                e => Try.Succ(-1)
            );

        MenuHelper.PrintResult("withCatch.Run()", withCatch.Run());

        // | Error - 에러 대체
        MenuHelper.PrintCode(@"// 원래 에러를 더 의미있는 에러로 대체
var withNewError = Try.lift(() => int.Parse(""abc""))
    | Error.New(""숫자가 아닌 입력입니다"");");

        var withNewError = Try.lift(() => int.Parse("abc"))
            | Error.New("숫자가 아닌 입력입니다");

        MenuHelper.PrintResult("withNewError.Run()", withNewError.Run());

        // | Pure - 성공값으로 대체
        MenuHelper.PrintCode(@"// 실패 시 기본 성공값으로 대체
var withDefault = Try.lift(() => int.Parse(""abc""))
    | Pure(0);");

        var withDefault = Try.lift(() => int.Parse("abc"))
            | Pure(0);

        MenuHelper.PrintResult("withDefault.Run()", withDefault.Run());

        // ============================================================
        // 5. 재시도 패턴
        // ============================================================
        MenuHelper.PrintSubHeader("5. 재시도 패턴");

        MenuHelper.PrintExplanation("| 연산자로 간단한 재시도 패턴을 구현할 수 있습니다.");
        MenuHelper.PrintBlankLines();

        MenuHelper.PrintCode(@"var attemptCount = 0;
var withRetry =
    Try.lift(() => FailingCall(ref attemptCount, failUntil: 2))
    | Try.lift(() => FailingCall(ref attemptCount, failUntil: 2))
    | Try.lift(() => FailingCall(ref attemptCount, failUntil: 2))
    | Try.Succ(""기본값"");");

        var attemptCount = 0;
        Console.WriteLine("  [재시도 실행]");
        var withRetry =
            Try.lift(() => FailingCall(ref attemptCount, failUntil: 2))
            | Try.lift(() => FailingCall(ref attemptCount, failUntil: 2))
            | Try.lift(() => FailingCall(ref attemptCount, failUntil: 2))
            | Try.Succ("기본값");

        MenuHelper.PrintResult("withRetry.Run()", withRetry.Run());
        MenuHelper.PrintResult("총 시도 횟수", attemptCount);

        // ============================================================
        // 6. 타입 변환 (ToOption, ToEither, ToFin)
        // ============================================================
        MenuHelper.PrintSubHeader("6. 타입 변환");

        MenuHelper.PrintExplanation("Try의 결과를 다른 모나드 타입으로 변환할 수 있습니다.");
        MenuHelper.PrintBlankLines();

        var trySuccess = Try.Succ(42);
        var tryFail = Try.lift(() => int.Parse("abc"));

        // Run() -> Fin
        MenuHelper.PrintCode("Fin<int> fin = trySuccess.Run();");
        Fin<int> fin = trySuccess.Run();
        MenuHelper.PrintResult("Try -> Fin (성공)", fin);

        Fin<int> finFail = tryFail.Run();
        MenuHelper.PrintResult("Try -> Fin (실패)", finFail);

        // ToOption
        MenuHelper.PrintCode("Option<int> opt = trySuccess.ToOption();");
        Option<int> opt = trySuccess.ToOption();
        MenuHelper.PrintResult("Try -> Option (성공)", opt);

        Option<int> optFail = tryFail.ToOption();
        MenuHelper.PrintResult("Try -> Option (실패)", optFail);

        // ToEither
        MenuHelper.PrintCode("Either<Error, int> either = trySuccess.ToEither();");
        Either<Error, int> either = trySuccess.ToEither();
        MenuHelper.PrintResult("Try -> Either (성공)", either);

        Either<Error, int> eitherFail = tryFail.ToEither();
        MenuHelper.PrintResult("Try -> Either (실패)", eitherFail);

        // ============================================================
        // 7. MapFail과 BindFail
        // ============================================================
        MenuHelper.PrintSubHeader("7. MapFail과 BindFail");

        MenuHelper.PrintExplanation("MapFail: 에러를 다른 에러로 변환");
        MenuHelper.PrintExplanation("BindFail: 실패 시 다른 Try로 복구");
        MenuHelper.PrintBlankLines();

        MenuHelper.PrintCode(@"// MapFail - 에러 변환
var mapFailed = Try.lift(() => int.Parse(""abc""))
    .MapFail(e => Error.New($""파싱 실패: {e.Message}""));");

        var mapFailed = Try.lift(() => int.Parse("abc"))
            .MapFail(e => Error.New($"파싱 실패: {e.Message}"));

        MenuHelper.PrintResult("mapFailed.Run()", mapFailed.Run());

        MenuHelper.PrintCode(@"// BindFail - 에러 복구
var recovered = Try.lift(() => int.Parse(""abc""))
    .BindFail(e => Try.Succ(0));");

        var recovered = Try.lift(() => int.Parse("abc"))
            .BindFail(e => Try.Succ(0));

        MenuHelper.PrintResult("recovered.Run()", recovered.Run());

        // ============================================================
        // 8. 복합 파이프라인 예제
        // ============================================================
        MenuHelper.PrintSubHeader("8. 복합 파이프라인 예제");

        MenuHelper.PrintExplanation("실무에서 자주 사용되는 복합 패턴입니다.");
        MenuHelper.PrintBlankLines();

        MenuHelper.PrintCode(@"// API 호출 -> 파싱 -> 검증 -> 폴백 체인
var result =
    (from data in FetchFromApi(""https://api.example.com/config"")
     from config in ParseConfig(data)
     from valid in ValidateConfig(config)
     select valid)
    | @catch<Try, AppConfig>(
        e => e.Message.Contains(""network""),
        _ => LoadFromCache()
    )
    | Try.Succ(AppConfig.Default);");

        Console.WriteLine("  [복합 파이프라인 실행]");
        var complexResult =
            (from data in FetchFromApi("https://api.example.com/config")
             from cfg in ParseConfig(data)
             from valid in ValidateConfig(cfg)
             select valid)
            | @catch<Try, AppConfig>(
                e => e.Message.Contains("network"),
                _ => LoadFromCache()
            )
            | Try.Succ(AppConfig.Default);

        MenuHelper.PrintResult("complexResult.Run()", complexResult.Run());

        // ============================================================
        // 9. BiBind - 양방향 바인딩
        // ============================================================
        MenuHelper.PrintSubHeader("9. BiBind - 양방향 바인딩");

        MenuHelper.PrintExplanation("BiBind로 성공/실패 모두 다른 Try로 변환할 수 있습니다.");
        MenuHelper.PrintBlankLines();

        MenuHelper.PrintCode(@"var bibound = Try.lift(() => int.Parse(""abc""))
    .BiBind(
        Succ: v => Try.Succ(v * 2),
        Fail: e => Try.Succ(0)
    );");

        var bibound = Try.lift(() => int.Parse("abc"))
            .BiBind(
                Succ: v => Try.Succ(v * 2),
                Fail: e => Try.Succ(0)
            );

        MenuHelper.PrintResult("bibound.Run()", bibound.Run());

        // 성공 케이스
        var biboundSucc = Try.lift(() => int.Parse("21"))
            .BiBind(
                Succ: v => Try.Succ(v * 2),
                Fail: e => Try.Succ(0)
            );

        MenuHelper.PrintResult("biboundSucc.Run()", biboundSucc.Run());

        MenuHelper.PrintSuccess("실무 Try 활용 학습 완료!");
    }

    // ============================================================
    // 헬퍼 타입 및 함수들
    // ============================================================

    private record AppConfig(string Name, int Version, bool Debug)
    {
        public static AppConfig Default => new("DefaultApp", 1, false);
    }

    private static Try<AppConfig> ParseConfig(string json) =>
        Try.lift(() => JsonSerializer.Deserialize<AppConfig>(json, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        })!);

    private static string FailingCall(ref int count, int failUntil)
    {
        count++;
        Console.WriteLine($"    시도 #{count}");
        if (count < failUntil)
            throw new Exception($"시도 {count} 실패");
        return $"성공 (시도 {count})";
    }

    private static Try<string> ReadFile(string path) =>
        Try.lift<string>(() =>
        {
            Console.WriteLine($"    파일 읽기 시도: {path}");
            throw new FileNotFoundException($"파일을 찾을 수 없습니다: {path}");
        });

    private static Try<T> ParseJson<T>(string json) =>
        Try.lift(() => JsonSerializer.Deserialize<T>(json)!);

    private static Try<AppConfig> Validate(AppConfig config) =>
        string.IsNullOrEmpty(config.Name)
            ? Try.Fail<AppConfig>(Error.New("설정 이름이 비어있습니다"))
            : Try.Succ(config);

    private static Try<AppConfig> ValidateConfig(AppConfig config) =>
        config.Version <= 0
            ? Try.Fail<AppConfig>(Error.New("버전은 양수여야 합니다"))
            : Try.Succ(config);

    private static Try<AppConfig> LoadAppConfig(string path) =>
        from json in ReadFile(path)
        from config in ParseJson<AppConfig>(json)
        from valid in Validate(config)
        select valid;

    private static Try<string> FetchFromApi(string url) =>
        Try.lift<string>(() =>
        {
            Console.WriteLine($"    API 호출: {url}");
            throw new Exception("network error: 연결 실패");
        });

    private static Try<AppConfig> LoadFromCache() =>
        Try.lift(() =>
        {
            Console.WriteLine("    캐시에서 로드");
            return new AppConfig("CachedApp", 1, false);
        });
}
