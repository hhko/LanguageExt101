using LanguageExt;
using LanguageExt101.Common;
using static LanguageExt.Prelude;

namespace LanguageExt101.Part1_Fundamentals.Chapter01_Introduction;

/// <summary>
/// Prelude 기본 사용법을 학습합니다.
///
/// 학습 목표:
/// - using static LanguageExt.Prelude 패턴 이해
/// - Prelude에서 제공하는 기본 함수들 소개
/// - 함수형 스타일 코드 작성의 기초
///
/// 핵심 개념:
/// Prelude는 LanguageExt의 핵심 함수들을 모아둔 정적 클래스입니다.
/// using static을 사용하면 Some(), None, Left(), Right() 등의 함수를
/// 클래스 이름 없이 바로 호출할 수 있어 함수형 스타일 코드를 작성하기 쉬워집니다.
///
/// Haskell의 Prelude 모듈에서 영감을 받아 만들어졌으며,
/// 함수형 프로그래밍에서 자주 사용되는 기본 함수들을 제공합니다.
/// </summary>
public static class E01_PreludeBasics
{
    /// <summary>
    /// 예제를 실행합니다.
    /// </summary>
    public static void Run()
    {
        MenuHelper.PrintHeader("Chapter 01-E01: Prelude 기본 사용법");

        // ============================================================
        // 1. using static 패턴
        // ============================================================
        MenuHelper.PrintSubHeader("1. using static 패턴");

        MenuHelper.PrintExplanation("파일 상단에 'using static LanguageExt.Prelude;'를 추가하면");
        MenuHelper.PrintExplanation("Prelude의 모든 함수를 접두사 없이 사용할 수 있습니다.");
        MenuHelper.PrintBlankLines();

        MenuHelper.PrintCode("// 일반적인 사용법:");
        MenuHelper.PrintCode("var option1 = LanguageExt.Prelude.Some(42);");
        MenuHelper.PrintBlankLines();

        MenuHelper.PrintCode("// using static 사용 시:");
        MenuHelper.PrintCode("var option2 = Some(42);");

        // Some은 Prelude의 함수입니다
        var someValue = Some(42);
        MenuHelper.PrintResult("Some(42)", someValue);

        // ============================================================
        // 2. 주요 Prelude 함수들 미리보기
        // ============================================================
        MenuHelper.PrintSubHeader("2. 주요 Prelude 함수들 미리보기");

        MenuHelper.PrintExplanation("Prelude는 다양한 범주의 함수들을 제공합니다:");
        MenuHelper.PrintBlankLines();

        // Option 관련
        MenuHelper.PrintCode("// Option 생성");
        var some = Some("Hello");
        Option<string> none = None;
        MenuHelper.PrintResult("Some(\"Hello\")", some);
        MenuHelper.PrintResult("None", none);
        MenuHelper.PrintBlankLines();

        // Either 관련
        MenuHelper.PrintCode("// Either 생성");
        var right = Right<string, int>(100);
        var left = Left<string, int>("에러 발생");
        MenuHelper.PrintResult("Right<string, int>(100)", right);
        MenuHelper.PrintResult("Left<string, int>(\"에러 발생\")", left);
        MenuHelper.PrintBlankLines();

        // 리스트/시퀀스 관련
        MenuHelper.PrintCode("// 불변 컬렉션 생성");
        var list = List(1, 2, 3, 4, 5);
        var seq = Seq(10, 20, 30);
        MenuHelper.PrintResult("List(1, 2, 3, 4, 5)", list);
        MenuHelper.PrintResult("Seq(10, 20, 30)", seq);
        MenuHelper.PrintBlankLines();

        // 튜플 관련
        MenuHelper.PrintCode("// 튜플 생성");
        var tuple = ("이름", 25);
        MenuHelper.PrintResult("(\"이름\", 25)", tuple);

        // ============================================================
        // 3. 왜 Prelude를 사용하는가?
        // ============================================================
        MenuHelper.PrintSubHeader("3. 왜 Prelude를 사용하는가?");

        MenuHelper.PrintExplanation("1. 간결한 코드: 타입 접두사 없이 함수를 호출할 수 있습니다.");
        MenuHelper.PrintExplanation("2. 함수형 스타일: Haskell 스타일의 자연스러운 코드 작성이 가능합니다.");
        MenuHelper.PrintExplanation("3. 일관성: 모든 LanguageExt 코드에서 동일한 패턴을 사용합니다.");
        MenuHelper.PrintExplanation("4. 발견 용이성: IDE 자동완성으로 사용 가능한 함수를 쉽게 찾을 수 있습니다.");

        // ============================================================
        // 4. Prelude 권장 사용법
        // ============================================================
        MenuHelper.PrintSubHeader("4. Prelude 권장 사용법");

        MenuHelper.PrintCode("// 파일 상단에 다음을 추가하세요:");
        MenuHelper.PrintCode("using LanguageExt;");
        MenuHelper.PrintCode("using static LanguageExt.Prelude;");
        MenuHelper.PrintBlankLines();

        MenuHelper.PrintExplanation("이렇게 하면 LanguageExt의 모든 타입과 Prelude 함수를 사용할 수 있습니다.");
        MenuHelper.PrintExplanation("앞으로의 모든 예제에서 이 패턴을 사용합니다.");

        MenuHelper.PrintSuccess("Prelude 기본 사용법 학습 완료!");
    }
}
