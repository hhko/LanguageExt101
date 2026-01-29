using System.Text.Json;
using LanguageExt;
using LanguageExt.Common;
using LanguageExt101.Common;
using static LanguageExt.Prelude;

namespace LanguageExt101.Part1_Fundamentals.Chapter04_FinAndError;

/// <summary>
/// 실습: Try를 활용한 설정 파일 로드 시스템
///
/// 시나리오:
/// - 설정 파일을 읽어 AppSettings로 파싱
/// - 파일이 없거나 JSON이 유효하지 않으면 기본 설정 사용
/// - 설정 검증 실패 시 에러 메시지 출력
/// </summary>
public static class Exercise04_TryPipeline
{
    public static void Run()
    {
        MenuHelper.PrintHeader("Exercise 04: Try를 활용한 설정 로드 시스템");

        MenuHelper.PrintExplanation("이 실습에서는 Try를 활용하여 안전한 설정 로드 시스템을 구현합니다.");
        MenuHelper.PrintExplanation("파일 읽기, JSON 파싱, 검증, 폴백 처리를 Try 파이프라인으로 처리합니다.");
        MenuHelper.PrintBlankLines();

        // ============================================================
        // 테스트 케이스 1: 정상 파일 로드
        // ============================================================
        MenuHelper.PrintSubHeader("테스트 1: 정상 설정 로드");

        var validJson = """
            {
                "AppName": "MyApplication",
                "MaxConnections": 100,
                "EnableLogging": true,
                "ApiEndpoint": "https://api.example.com"
            }
            """;

        Console.WriteLine("  [입력 JSON]");
        Console.WriteLine($"  {validJson.Replace("\n", "\n  ")}");
        MenuHelper.PrintBlankLines();

        var result1 = LoadSettings(() => validJson);
        MenuHelper.PrintResult("결과", FormatResult(result1));

        // ============================================================
        // 테스트 케이스 2: 파일 없음 -> 기본 설정
        // ============================================================
        MenuHelper.PrintSubHeader("테스트 2: 파일 없음 -> 기본 설정 사용");

        var result2 = LoadSettings(() => throw new FileNotFoundException("설정 파일을 찾을 수 없습니다"));
        MenuHelper.PrintResult("결과", FormatResult(result2));

        // ============================================================
        // 테스트 케이스 3: JSON 파싱 오류 -> 기본 설정
        // ============================================================
        MenuHelper.PrintSubHeader("테스트 3: JSON 오류 -> 기본 설정 사용");

        var invalidJson = """{"AppName": "Test", "MaxConnections":}""";
        Console.WriteLine($"  [잘못된 JSON] {invalidJson}");
        MenuHelper.PrintBlankLines();

        var result3 = LoadSettings(() => invalidJson);
        MenuHelper.PrintResult("결과", FormatResult(result3));

        // ============================================================
        // 테스트 케이스 4: 검증 실패
        // ============================================================
        MenuHelper.PrintSubHeader("테스트 4: 검증 실패 (MaxConnections가 0)");

        var invalidSettings = """
            {
                "AppName": "TestApp",
                "MaxConnections": 0,
                "EnableLogging": true,
                "ApiEndpoint": "https://api.test.com"
            }
            """;

        Console.WriteLine("  [입력 JSON]");
        Console.WriteLine($"  {invalidSettings.Replace("\n", "\n  ")}");
        MenuHelper.PrintBlankLines();

        var result4 = LoadSettingsStrict(() => invalidSettings);
        MenuHelper.PrintResult("결과", FormatResult(result4));

        // ============================================================
        // 테스트 케이스 5: 폴백 체인
        // ============================================================
        MenuHelper.PrintSubHeader("테스트 5: 폴백 체인 (여러 설정 파일 시도)");

        MenuHelper.PrintCode(@"// 우선순위: prod.json -> dev.json -> 기본 설정
var config =
    LoadFromFile(""config.prod.json"")
    | LoadFromFile(""config.dev.json"")
    | LoadFromFile(""config.json"")
    | Try.Succ(AppSettings.Default);");

        Console.WriteLine("  [폴백 체인 실행]");
        var result5 = LoadWithFallbackChain();
        MenuHelper.PrintResult("최종 결과", FormatResult(result5));

        // ============================================================
        // 테스트 케이스 6: 에러 로깅
        // ============================================================
        MenuHelper.PrintSubHeader("테스트 6: 에러 로깅");

        MenuHelper.PrintExplanation("IfFail을 사용하여 에러를 로깅하면서 폴백 처리합니다.");
        MenuHelper.PrintBlankLines();

        var result6 = LoadWithErrorLogging(() => throw new Exception("네트워크 오류"));
        MenuHelper.PrintResult("결과", FormatResult(result6));

        // ============================================================
        // 실습 요약
        // ============================================================
        MenuHelper.PrintSubHeader("실습 요약");

        MenuHelper.PrintExplanation("이 실습에서 배운 내용:");
        MenuHelper.PrintExplanation("1. Try.lift(() => ...)로 예외를 던지는 함수를 안전하게 래핑");
        MenuHelper.PrintExplanation("2. LINQ 구문으로 깔끔한 파이프라인 구성");
        MenuHelper.PrintExplanation("3. | 연산자로 폴백 체인 구현");
        MenuHelper.PrintExplanation("4. BiBind로 조건부 복구");
        MenuHelper.PrintExplanation("5. Match로 최종 결과 처리");

        MenuHelper.PrintSuccess("Try 파이프라인 실습 완료!");
    }

    // ============================================================
    // 설정 타입
    // ============================================================
    private record AppSettings(
        string AppName,
        int MaxConnections,
        bool EnableLogging,
        string ApiEndpoint)
    {
        public static AppSettings Default => new(
            AppName: "DefaultApp",
            MaxConnections: 10,
            EnableLogging: false,
            ApiEndpoint: "http://localhost:8080"
        );
    }

    // ============================================================
    // 핵심 함수들
    // ============================================================

    /// <summary>
    /// 설정을 로드하고, 실패 시 기본 설정을 반환합니다.
    /// </summary>
    private static Fin<AppSettings> LoadSettings(Func<string> readJson)
    {
        var pipeline =
            from json in Try.lift(readJson)
            from settings in ParseSettings(json)
            select settings;

        // 실패 시 기본 설정으로 폴백
        return (pipeline | Try.Succ(AppSettings.Default)).Run();
    }

    /// <summary>
    /// 설정을 로드하고 검증합니다. 검증 실패 시 에러를 반환합니다.
    /// </summary>
    private static Fin<AppSettings> LoadSettingsStrict(Func<string> readJson)
    {
        var pipeline =
            from json in Try.lift(readJson)
            from settings in ParseSettings(json)
            from validated in ValidateSettings(settings)
            select validated;

        return pipeline.Run();
    }

    /// <summary>
    /// JSON 문자열을 AppSettings로 파싱합니다.
    /// </summary>
    private static Try<AppSettings> ParseSettings(string json) =>
        Try.lift(() => JsonSerializer.Deserialize<AppSettings>(json, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        })!);

    /// <summary>
    /// 설정을 검증합니다.
    /// </summary>
    private static Try<AppSettings> ValidateSettings(AppSettings settings)
    {
        if (string.IsNullOrWhiteSpace(settings.AppName))
            return Error.New("앱 이름이 비어있습니다");

        if (settings.MaxConnections <= 0)
            return Error.New("MaxConnections는 0보다 커야 합니다");

        if (string.IsNullOrWhiteSpace(settings.ApiEndpoint))
            return Error.New("API 엔드포인트가 비어있습니다");

        return Try.Succ(settings);
    }

    /// <summary>
    /// 파일에서 설정을 로드합니다 (시뮬레이션).
    /// </summary>
    private static Try<AppSettings> LoadFromFile(string path) =>
        Try.lift<AppSettings>(() =>
        {
            Console.WriteLine($"    시도: {path}");
            throw new FileNotFoundException($"파일을 찾을 수 없습니다: {path}");
        });

    /// <summary>
    /// 폴백 체인으로 설정을 로드합니다.
    /// </summary>
    private static Fin<AppSettings> LoadWithFallbackChain()
    {
        var pipeline =
            LoadFromFile("config.prod.json")
            | LoadFromFile("config.dev.json")
            | LoadFromFile("config.json")
            | Try.lift(() =>
            {
                Console.WriteLine("    기본 설정 사용");
                return AppSettings.Default;
            });

        return pipeline.Run();
    }

    /// <summary>
    /// 에러를 로깅하면서 설정을 로드합니다.
    /// </summary>
    private static Fin<AppSettings> LoadWithErrorLogging(Func<string> readJson)
    {
        var result = Try.lift(readJson)
            .Bind(json => ParseSettings(json))
            .Run();

        return result.Match(
            Succ: s => Fin.Succ(s),
            Fail: e =>
            {
                Console.WriteLine($"    [경고] 설정 로드 실패: {e.Message}");
                Console.WriteLine("    [정보] 기본 설정을 사용합니다");
                return Fin.Succ(AppSettings.Default);
            }
        );
    }

    /// <summary>
    /// 결과를 포맷팅합니다.
    /// </summary>
    private static string FormatResult(Fin<AppSettings> result) =>
        result.Match(
            Succ: s => $"AppName={s.AppName}, MaxConnections={s.MaxConnections}, " +
                       $"EnableLogging={s.EnableLogging}, ApiEndpoint={s.ApiEndpoint}",
            Fail: e => $"[실패] {e.Message}"
        );
}
