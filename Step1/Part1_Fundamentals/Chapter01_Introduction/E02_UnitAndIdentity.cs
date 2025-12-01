using LanguageExt;
using LanguageExt101.Common;
using static LanguageExt.Prelude;

namespace LanguageExt101.Part1_Fundamentals.Chapter01_Introduction;

/// <summary>
/// unit, identity, constant 함수를 학습합니다.
///
/// 학습 목표:
/// - Unit 타입의 개념과 사용법
/// - identity 함수의 역할
/// - constant 함수의 활용
/// - fun() 헬퍼 함수
///
/// 핵심 개념:
/// 함수형 프로그래밍에서는 모든 것이 값이며, "아무것도 없음"도 값으로 표현됩니다.
/// Unit은 의미 있는 값이 없음을 나타내는 타입으로, C#의 void 대신 사용됩니다.
/// identity와 constant는 함수 합성과 변환에서 기본 빌딩 블록 역할을 합니다.
/// </summary>
public static class E02_UnitAndIdentity
{
    /// <summary>
    /// 예제를 실행합니다.
    /// </summary>
    public static void Run()
    {
        MenuHelper.PrintHeader("Chapter 01-E02: unit, identity, constant");

        // ============================================================
        // 1. Unit 타입
        // ============================================================
        MenuHelper.PrintSubHeader("1. Unit 타입");

        MenuHelper.PrintExplanation("Unit은 '값이 없음'을 나타내는 타입입니다.");
        MenuHelper.PrintExplanation("C#의 void는 타입으로 사용할 수 없지만, Unit은 가능합니다.");
        MenuHelper.PrintExplanation("이를 통해 void 반환 함수도 제네릭하게 다룰 수 있습니다.");
        MenuHelper.PrintBlankLines();

        // Unit 값 생성
        MenuHelper.PrintCode("// Unit 값 생성 방법들");
        var unit1 = unit;                    // Prelude의 unit 상수
        var unit2 = Unit.Default;            // Unit 타입의 기본값
        var unit3 = default(Unit);           // default 키워드 사용

        MenuHelper.PrintResult("unit", unit1);
        MenuHelper.PrintResult("Unit.Default", unit2);
        MenuHelper.PrintResult("default(Unit)", unit3);
        MenuHelper.PrintBlankLines();

        // Unit을 반환하는 함수
        MenuHelper.PrintCode("// void 대신 Unit을 반환하면 함수 합성이 가능해집니다");

        Func<string, Unit> logMessage = msg =>
        {
            Console.WriteLine($"  [LOG] {msg}");
            return unit;
        };

        logMessage("Unit을 반환하는 로깅 함수");
        MenuHelper.PrintBlankLines();

        // Option<Unit> 예시
        MenuHelper.PrintCode("// Option<Unit>: 작업의 성공/실패만 표현");
        Option<Unit> successResult = Some(unit);
        Option<Unit> failureResult = None;

        MenuHelper.PrintResult("성공한 작업 (Some(unit))", successResult);
        MenuHelper.PrintResult("실패한 작업 (None)", failureResult);

        // ============================================================
        // 2. identity 함수
        // ============================================================
        MenuHelper.PrintSubHeader("2. identity 함수");

        MenuHelper.PrintExplanation("identity는 입력을 그대로 반환하는 함수입니다.");
        MenuHelper.PrintExplanation("언뜻 쓸모없어 보이지만, 함수 합성에서 중요한 역할을 합니다.");
        MenuHelper.PrintExplanation("Map이나 Fold에서 '변환 없음'을 표현할 때 유용합니다.");
        MenuHelper.PrintBlankLines();

        MenuHelper.PrintCode("// identity 함수: T -> T");
        var num = 42;
        var same = identity(num);
        MenuHelper.PrintResult("identity(42)", same);

        var text = "Hello";
        var sameText = identity(text);
        MenuHelper.PrintResult("identity(\"Hello\")", sameText);
        MenuHelper.PrintBlankLines();

        // identity의 실용적 사용 예
        MenuHelper.PrintCode("// 실용적 사용: Match에서 Some 값을 그대로 사용");
        var option = Some(100);
        var result = option.Match(
            Some: identity,    // Some일 때 값을 그대로 반환
            None: () => 0      // None일 때 기본값 반환
        );
        MenuHelper.PrintResult("option.Match(Some: identity, None: () => 0)", result);
        MenuHelper.PrintBlankLines();

        // Fold에서의 identity
        MenuHelper.PrintCode("// Fold에서 identity 사용");
        var numbers = List(1, 2, 3, 4, 5);
        // 첫 번째 요소 또는 기본값 가져오기
        var first = numbers.Match(
            Empty: () => 0,
            More: (head, _) => head
        );
        MenuHelper.PrintResult("numbers의 첫 번째 요소", first);

        // ============================================================
        // 3. constant 함수
        // ============================================================
        MenuHelper.PrintSubHeader("3. constant 함수");

        MenuHelper.PrintExplanation("constant는 항상 같은 값을 반환하는 함수를 만듭니다.");
        MenuHelper.PrintExplanation("입력을 무시하고 미리 정해진 값만 반환합니다.");
        MenuHelper.PrintExplanation("Map이나 Select에서 모든 요소를 같은 값으로 변환할 때 유용합니다.");
        MenuHelper.PrintBlankLines();

        MenuHelper.PrintCode("// constant 함수: 항상 같은 값 반환");
        Func<string, int> alwaysFortyTwo = _ => 42;
        MenuHelper.PrintResult("alwaysFortyTwo(\"무엇을 넣어도\")", alwaysFortyTwo("무엇을 넣어도"));
        MenuHelper.PrintResult("alwaysFortyTwo(\"42를 반환\")", alwaysFortyTwo("42를 반환"));
        MenuHelper.PrintBlankLines();

        // constant의 실용적 사용
        MenuHelper.PrintCode("// 실용적 사용: 모든 요소를 같은 값으로 변환");
        var items = List("a", "b", "c");
        var allOnes = items.Map(_ => 1);
        MenuHelper.PrintResult("items.Map(_ => 1)", allOnes);

        // ============================================================
        // 4. fun() 헬퍼 함수
        // ============================================================
        MenuHelper.PrintSubHeader("4. fun() 헬퍼 함수");

        MenuHelper.PrintExplanation("fun()은 람다를 Func<>로 변환하는 헬퍼입니다.");
        MenuHelper.PrintExplanation("C# 컴파일러가 람다 타입을 추론하지 못할 때 유용합니다.");
        MenuHelper.PrintBlankLines();

        MenuHelper.PrintCode("// fun()을 사용한 함수 정의");
        var add = fun((int a, int b) => a + b);
        var multiply = fun((int a, int b) => a * b);

        MenuHelper.PrintResult("add(3, 5)", add(3, 5));
        MenuHelper.PrintResult("multiply(4, 7)", multiply(4, 7));
        MenuHelper.PrintBlankLines();

        // Action을 Unit 반환 함수로 변환
        MenuHelper.PrintCode("// Action을 Func<Unit>으로 변환");
        Action<string> printAction = s => Console.WriteLine($"  Action: {s}");
        Func<string, Unit> printFunc = s => { printAction(s); return unit; };

        printFunc("이제 합성 가능한 함수입니다");

        // ============================================================
        // 5. ignore 함수
        // ============================================================
        MenuHelper.PrintSubHeader("5. ignore 함수");

        MenuHelper.PrintExplanation("ignore는 값을 무시하고 Unit을 반환합니다.");
        MenuHelper.PrintExplanation("반환값이 필요 없을 때 파이프라인을 유지하는 데 사용합니다.");
        MenuHelper.PrintBlankLines();

        MenuHelper.PrintCode("// ignore: 값을 무시하고 Unit 반환");
        var ignoredResult = ignore(42);
        MenuHelper.PrintResult("ignore(42)", ignoredResult);

        MenuHelper.PrintSuccess("unit, identity, constant 학습 완료!");
    }
}
