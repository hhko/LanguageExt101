using LanguageExt;
using LanguageExt.Common;
using LanguageExt101.Common;
using static LanguageExt.Prelude;

namespace LanguageExt101.Part2_Intermediate.Chapter08_Guard;

/// <summary>
/// 조건부 실행 패턴을 학습합니다.
///
/// 학습 목표:
/// - 조건에 따른 동작 실행
/// - 선언적 조건 처리
/// - Option/Either와 함께 사용
///
/// 핵심 개념:
/// 함수형 프로그래밍에서는 조건부 실행을 선언적으로 표현합니다.
/// if문 대신 함수형 스타일의 조건 처리를 사용할 수 있습니다.
/// </summary>
public static class E02_WhenUnless
{
    /// <summary>
    /// 예제를 실행합니다.
    /// </summary>
    public static void Run()
    {
        MenuHelper.PrintHeader("Chapter 08-E02: 조건부 실행 패턴");

        // ============================================================
        // 1. 조건부 실행 기본
        // ============================================================
        MenuHelper.PrintSubHeader("1. 조건부 실행 기본");

        MenuHelper.PrintExplanation("조건에 따라 다른 동작을 실행합니다.");
        MenuHelper.PrintBlankLines();

        int score = 95;

        // 일반 C# 패턴
        if (score >= 90) Console.WriteLine("  A등급입니다!");
        if (score >= 80 && score < 90) Console.WriteLine("  B등급입니다.");
        if (score < 80) Console.WriteLine("  C등급 이하입니다.");

        // ============================================================
        // 2. Action을 사용한 조건부 실행
        // ============================================================
        MenuHelper.PrintSubHeader("2. Action을 사용한 조건부 실행");

        MenuHelper.PrintExplanation("Action을 변수로 만들어 조건부로 실행합니다.");
        MenuHelper.PrintBlankLines();

        Action logSuccess = () => Console.WriteLine("  성공!");
        Action logFailure = () => Console.WriteLine("  실패!");

        bool isSuccess = true;
        var action = isSuccess ? logSuccess : logFailure;
        action();

        // ============================================================
        // 3. 삼항 연산자 패턴
        // ============================================================
        MenuHelper.PrintSubHeader("3. 삼항 연산자 패턴");

        MenuHelper.PrintExplanation("조건에 따라 다른 값을 반환합니다.");
        MenuHelper.PrintBlankLines();

        int age = 25;
        var status1 = age >= 18 ? "성인" : "미성년자";
        MenuHelper.PrintResult($"age={age}일 때 상태", status1);

        age = 15;
        var status2 = age >= 18 ? "성인" : "미성년자";
        MenuHelper.PrintResult($"age={age}일 때 상태", status2);

        // ============================================================
        // 4. 지연 평가 패턴
        // ============================================================
        MenuHelper.PrintSubHeader("4. 지연 평가 패턴");

        MenuHelper.PrintExplanation("계산 비용이 큰 경우 Func를 사용하여 지연 평가합니다.");
        MenuHelper.PrintBlankLines();

        bool needExpensiveCalc = false;

        Func<string> expensiveResult = () =>
        {
            Console.WriteLine("  [비용 큰 계산 실행]");
            return "비싼 결과";
        };

        Func<string> cheapResult = () => "기본 결과";

        var result = needExpensiveCalc ? expensiveResult() : cheapResult();
        MenuHelper.PrintResult("지연 평가 결과", result);

        // ============================================================
        // 5. 실전 예제: 권한 검사
        // ============================================================
        MenuHelper.PrintSubHeader("5. 실전 예제: 권한 검사");

        var adminUser = new User("admin", "Admin", true);
        var normalUser = new User("user1", "User", false);

        CheckPermissions(adminUser);
        CheckPermissions(normalUser);

        // ============================================================
        // 6. Option과 함께 사용
        // ============================================================
        MenuHelper.PrintSubHeader("6. Option과 함께 사용");

        MenuHelper.PrintExplanation("Option의 IfSome/IfNone으로 조건부 실행합니다.");
        MenuHelper.PrintBlankLines();

        var maybeValue = Some(42);
        Option<int> noValue = None;

        maybeValue.IfSome(v => Console.WriteLine($"  양수 값: {v}"));
        noValue.IfNone(() => Console.WriteLine("  값이 없습니다"));

        // ============================================================
        // 7. 복합 조건
        // ============================================================
        MenuHelper.PrintSubHeader("7. 복합 조건");

        var order = new Order("ORD001", 150000m, "VIP", true);

        // 복합 조건 검사
        if (order.Amount >= 100000m && order.CustomerTier == "VIP")
            Console.WriteLine($"  주문 {order.Id}: VIP 대량 주문 - 무료 배송");

        if (order.IsPriority && order.Amount < 50000m)
            Console.WriteLine($"  주문 {order.Id}: 긴급 배송 추가 요금");

        if (!order.IsPriority)
            Console.WriteLine($"  주문 {order.Id}: 일반 배송");

        // ============================================================
        // 8. 함수형 조건 처리 패턴
        // ============================================================
        MenuHelper.PrintSubHeader("8. 함수형 조건 처리 패턴");

        MenuHelper.PrintExplanation("Match 패턴으로 조건을 선언적으로 처리합니다.");
        MenuHelper.PrintBlankLines();

        var debugMode = true;

        var modeMessage = debugMode switch
        {
            true => "[DEBUG] 디버그 모드 활성화",
            false => "[INFO] 프로덕션 모드"
        };
        Console.WriteLine($"  {modeMessage}");

        MenuHelper.PrintSuccess("조건부 실행 패턴 학습 완료!");
    }

    private record User(string Id, string Name, bool IsAdmin);
    private record Order(string Id, decimal Amount, string CustomerTier, bool IsPriority);

    private static void CheckPermissions(User user)
    {
        Console.WriteLine($"\n  사용자: {user.Name}");

        if (user.IsAdmin)
            Console.WriteLine("    - 관리자 권한 있음");
        else
            Console.WriteLine("    - 일반 사용자 권한");

        var accessLevel = user.IsAdmin ? "전체 접근" : "제한된 접근";
        Console.WriteLine($"    - 접근 레벨: {accessLevel}");
    }
}
