using LanguageExt;
using LanguageExt.Common;
using LanguageExt101.Common;
using static LanguageExt.Prelude;

namespace LanguageExt101.Part2_Intermediate.Chapter08_Guard;

/// <summary>
/// guard 기본 사용법을 학습합니다.
///
/// 학습 목표:
/// - guard 함수의 개념
/// - 조건부 모나드 흐름 제어
/// - guardnot 사용
/// - Option/Either에서의 guard
///
/// 핵심 개념:
/// guard는 조건이 거짓이면 모나드 흐름을 중단합니다.
/// LINQ의 where와 유사하지만, 모나드 내부에서 사용됩니다.
/// </summary>
public static class E01_GuardBasics
{
    /// <summary>
    /// 예제를 실행합니다.
    /// </summary>
    public static void Run()
    {
        MenuHelper.PrintHeader("Chapter 08-E01: guard 기본");

        // ============================================================
        // 1. guard의 개념
        // ============================================================
        MenuHelper.PrintSubHeader("1. guard의 개념");

        MenuHelper.PrintExplanation("guard는 조건이 참이면 계속, 거짓이면 중단합니다.");
        MenuHelper.PrintExplanation("LINQ의 where와 유사하지만 모나드 문맥에서 사용됩니다.");
        MenuHelper.PrintBlankLines();

        MenuHelper.PrintCode("// LINQ where 스타일");
        var numbers = Seq(1, 2, 3, 4, 5, 6, 7, 8, 9, 10);
        var evens = numbers.Filter(n => n % 2 == 0);
        MenuHelper.PrintResult("Filter(짝수)", evens);

        // ============================================================
        // 2. Option에서 guard
        // ============================================================
        MenuHelper.PrintSubHeader("2. Option에서 guard 사용");

        MenuHelper.PrintExplanation("Option 모나드에서 guard를 사용합니다.");
        MenuHelper.PrintBlankLines();

        // guard 없이
        var withoutGuard =
            from x in Some(10)
            select x * 2;
        MenuHelper.PrintResult("guard 없이", withoutGuard);

        // guard로 조건 검사
        var withGuard =
            from x in Some(10)
            where x > 5  // LINQ where는 guard와 유사
            select x * 2;
        MenuHelper.PrintResult("where x > 5", withGuard);

        // 조건 불만족 시
        var failedGuard =
            from x in Some(3)
            where x > 5  // 조건 실패 → None
            select x * 2;
        MenuHelper.PrintResult("Some(3) where x > 5", failedGuard);

        // ============================================================
        // 3. 체인에서 guard
        // ============================================================
        MenuHelper.PrintSubHeader("3. 체인에서 guard");

        MenuHelper.PrintExplanation("여러 조건을 체인으로 검사합니다.");
        MenuHelper.PrintBlankLines();

        var multipleConditions =
            from x in Some(15)
            where x > 10
            where x < 20
            where x % 5 == 0
            select $"유효한 값: {x}";

        MenuHelper.PrintResult("여러 조건 모두 만족", multipleConditions);

        var failsOne =
            from x in Some(25)  // 20보다 큼
            where x > 10
            where x < 20  // 실패!
            where x % 5 == 0
            select $"유효한 값: {x}";

        MenuHelper.PrintResult("하나의 조건 실패", failsOne);

        // ============================================================
        // 4. Either에서 guard 패턴
        // ============================================================
        MenuHelper.PrintSubHeader("4. Either에서 guard 패턴");

        MenuHelper.PrintExplanation("Either에서 조건 검사 패턴입니다.");
        MenuHelper.PrintBlankLines();

        var eitherResult = ValidateAge(25);
        MenuHelper.PrintResult("ValidateAge(25)", eitherResult);

        var eitherFail = ValidateAge(15);
        MenuHelper.PrintResult("ValidateAge(15)", eitherFail);

        // ============================================================
        // 5. 실전 예제: 주문 검증
        // ============================================================
        MenuHelper.PrintSubHeader("5. 실전 예제: 주문 검증");

        var validOrder = ProcessOrder("P001", 5, "VIP");
        var invalidQuantity = ProcessOrder("P001", 0, "VIP");
        var invalidTier = ProcessOrder("P001", 100, "일반");

        MenuHelper.PrintResult("유효한 주문", validOrder);
        MenuHelper.PrintResult("수량 0", invalidQuantity);
        MenuHelper.PrintResult("대량 주문인데 VIP 아님", invalidTier);

        // ============================================================
        // 6. guard vs Filter
        // ============================================================
        MenuHelper.PrintSubHeader("6. guard vs Filter");

        MenuHelper.PrintExplanation("guard: 모나드 내부에서 조건 검사");
        MenuHelper.PrintExplanation("Filter: 컬렉션에서 요소 필터링");
        MenuHelper.PrintBlankLines();

        // Filter: 컬렉션 요소 필터링
        var filtered = Seq(1, 2, 3, 4, 5).Filter(x => x > 2);
        MenuHelper.PrintResult("Seq.Filter(x > 2)", filtered);

        // LINQ where: Option 내부에서 조건
        var guarded =
            from x in Some(3)
            where x > 2
            select x;
        MenuHelper.PrintResult("Option where (x > 2)", guarded);

        MenuHelper.PrintSuccess("guard 기본 학습 완료!");
    }

    private static Either<string, int> ValidateAge(int age)
    {
        if (age < 0)
            return Left("나이는 음수일 수 없습니다");
        if (age < 18)
            return Left("성인만 이용 가능합니다");
        return Right(age);
    }

    private static Option<string> ProcessOrder(string productId, int quantity, string customerTier)
    {
        return
            from p in Optional(productId).Filter(id => !string.IsNullOrEmpty(id))
            where quantity > 0
            where quantity < 50 || customerTier == "VIP"  // 대량 주문은 VIP만
            select $"주문 접수: {p} x {quantity}";
    }
}
