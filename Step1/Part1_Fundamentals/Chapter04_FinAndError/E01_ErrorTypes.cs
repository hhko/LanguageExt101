using LanguageExt;
using LanguageExt.Common;
using LanguageExt101.Common;
using static LanguageExt.Prelude;

namespace LanguageExt101.Part1_Fundamentals.Chapter04_FinAndError;

/// <summary>
/// LanguageExt의 Error 타입 종류를 학습합니다.
/// </summary>
public static class E01_ErrorTypes
{
    public static void Run()
    {
        MenuHelper.PrintHeader("Chapter 04-E01: Error 타입 종류");

        // ============================================================
        // 1. Error 기본 생성
        // ============================================================
        MenuHelper.PrintSubHeader("1. Error 기본 생성");

        MenuHelper.PrintExplanation("Error는 다양한 방법으로 생성할 수 있습니다.");
        MenuHelper.PrintBlankLines();

        // 문자열로 생성
        MenuHelper.PrintCode("var error1 = Error.New(\"간단한 에러 메시지\");");
        var error1 = Error.New("간단한 에러 메시지");
        MenuHelper.PrintResult("문자열 에러", error1.Message);

        // 코드와 메시지
        MenuHelper.PrintCode("var error2 = Error.New(404, \"리소스를 찾을 수 없습니다\");");
        var error2 = Error.New(404, "리소스를 찾을 수 없습니다");
        MenuHelper.PrintResult("코드 + 메시지", error2.Message);
        MenuHelper.PrintResult("  - Code", error2.Code);

        // ============================================================
        // 2. Exception으로부터 Error 생성
        // ============================================================
        MenuHelper.PrintSubHeader("2. Exception으로부터 Error 생성");

        try
        {
            throw new InvalidOperationException("시스템 오류 발생");
        }
        catch (Exception ex)
        {
            MenuHelper.PrintCode("var exceptional = Error.New(exception);");
            var exceptional = Error.New(ex);
            MenuHelper.PrintResult("Exception 에러", exceptional.Message);
        }

        // ============================================================
        // 3. Error 결합
        // ============================================================
        MenuHelper.PrintSubHeader("3. Error 결합");

        MenuHelper.PrintExplanation("+ 연산자로 여러 에러를 결합할 수 있습니다.");
        MenuHelper.PrintBlankLines();

        var error3 = Error.New("이름은 필수입니다");
        var error4 = Error.New("이메일 형식이 올바르지 않습니다");
        var error5 = Error.New("비밀번호는 8자 이상이어야 합니다");

        MenuHelper.PrintCode("var combined = error1 + error2 + error3;");
        var combined = error3 + error4 + error5;
        MenuHelper.PrintResult("결합된 에러", combined.Message);

        // ============================================================
        // 4. 실전 예제
        // ============================================================
        MenuHelper.PrintSubHeader("4. 실전 예제: 에러 처리 패턴");

        var result1 = ProcessRequest(new Request("valid_user", "read"));
        var result2 = ProcessRequest(new Request("invalid_user", "read"));

        result1.Match(
            Right: r => MenuHelper.PrintSuccess($"성공: {r}"),
            Left: e => MenuHelper.PrintError($"실패 [{e.Code}]: {e.Message}")
        );

        result2.Match(
            Right: r => MenuHelper.PrintSuccess($"성공: {r}"),
            Left: e => MenuHelper.PrintError($"실패 [{e.Code}]: {e.Message}")
        );

        MenuHelper.PrintSuccess("Error 타입 종류 학습 완료!");
    }

    private record Request(string UserId, string Action);

    private static Either<Error, string> ProcessRequest(Request request)
    {
        if (request.UserId != "valid_user")
            return Error.New(404, $"사용자를 찾을 수 없습니다 (ID: {request.UserId})");

        if (request.Action == "delete")
            return Error.New(403, "권한이 없습니다");

        return Right($"요청 처리 완료: {request.Action}");
    }
}
