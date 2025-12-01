using LanguageExt;
using LanguageExt101.Common;
using static LanguageExt.Prelude;

namespace LanguageExt101.Part1_Fundamentals.Chapter02_Option;

/// <summary>
/// Option의 Bind(SelectMany)와 LINQ 통합을 학습합니다.
///
/// 학습 목표:
/// - Bind(FlatMap)의 개념 이해
/// - Map vs Bind의 차이점
/// - LINQ 쿼리 구문으로 Option 다루기
/// - 중첩된 Option 처리
///
/// 핵심 개념:
/// Bind는 모나드의 핵심 연산으로, Option<A>를 받아서 Option<B>를 반환하는
/// 함수를 적용합니다. Map과 달리 결과가 이미 Option일 때 사용합니다.
///
/// LINQ의 from ... select 구문을 사용하면 여러 Option을 자연스럽게 조합할 수 있습니다.
/// </summary>
public static class E03_BindAndLinq
{
    /// <summary>
    /// 예제를 실행합니다.
    /// </summary>
    public static void Run()
    {
        MenuHelper.PrintHeader("Chapter 02-E03: Bind와 LINQ");

        // ============================================================
        // 1. Map의 문제점: 중첩된 Option
        // ============================================================
        MenuHelper.PrintSubHeader("1. Map의 문제점: 중첩된 Option");

        MenuHelper.PrintExplanation("Map을 사용하면 Option<Option<T>>가 될 수 있습니다.");
        MenuHelper.PrintExplanation("이런 중첩은 다루기 어렵습니다.");
        MenuHelper.PrintBlankLines();

        // Map을 사용한 경우 (문제 상황)
        var userId = Some(1);

        MenuHelper.PrintCode("// Map 사용 시 중첩 발생");
        MenuHelper.PrintCode("var nested = userId.Map(id => FindUser(id));");
        MenuHelper.PrintCode("// 결과: Option<Option<User>>");

        var nested = userId.Map(id => FindUser(id));
        MenuHelper.PrintResult("Map 결과 (중첩됨)", nested);
        MenuHelper.PrintExplanation("Option<Option<User>>는 사용하기 불편합니다");

        // ============================================================
        // 2. Bind로 중첩 해결
        // ============================================================
        MenuHelper.PrintSubHeader("2. Bind로 중첩 해결");

        MenuHelper.PrintExplanation("Bind는 결과를 자동으로 평탄화(flatten)합니다.");
        MenuHelper.PrintExplanation("Option<A> → (A → Option<B>) → Option<B>");
        MenuHelper.PrintBlankLines();

        MenuHelper.PrintCode("var flat = userId.Bind(id => FindUser(id));");
        var flat = userId.Bind(id => FindUser(id));
        MenuHelper.PrintResult("Bind 결과 (평탄화됨)", flat);
        MenuHelper.PrintBlankLines();

        // None인 경우
        Option<int> noUserId = None;
        var noUser = noUserId.Bind(id => FindUser(id));
        MenuHelper.PrintResult("None.Bind(FindUser)", noUser);

        // ============================================================
        // 3. Map vs Bind 비교
        // ============================================================
        MenuHelper.PrintSubHeader("3. Map vs Bind 비교");

        MenuHelper.PrintCode("// Map: 일반 함수 적용 (A → B)");
        MenuHelper.PrintCode("// Option<A>.Map(A → B) = Option<B>");
        MenuHelper.PrintBlankLines();

        MenuHelper.PrintCode("// Bind: Option 반환 함수 적용 (A → Option<B>)");
        MenuHelper.PrintCode("// Option<A>.Bind(A → Option<B>) = Option<B>");
        MenuHelper.PrintBlankLines();

        var number = Some(10);

        // Map: int → string (일반 함수)
        var mapped = number.Map(n => $"숫자: {n}");
        MenuHelper.PrintResult("Map(n => $\"숫자: {n}\")", mapped);

        // Bind: int → Option<string> (Option 반환 함수)
        var bound = number.Bind(n => n > 5 ? Some($"큰 숫자: {n}") : None);
        MenuHelper.PrintResult("Bind(n => n > 5 ? Some(...) : None)", bound);

        // ============================================================
        // 4. Bind 체이닝
        // ============================================================
        MenuHelper.PrintSubHeader("4. Bind 체이닝");

        MenuHelper.PrintExplanation("Bind를 연속으로 사용하여 복잡한 흐름을 처리합니다.");
        MenuHelper.PrintExplanation("중간에 하나라도 None이면 전체가 None이 됩니다.");
        MenuHelper.PrintBlankLines();

        MenuHelper.PrintCode("// 사용자 → 부서 → 매니저 체이닝");
        var manager = FindUser(1)
            .Bind(user => FindDepartment(user.DepartmentId))
            .Bind(dept => FindUser(dept.ManagerId));

        MenuHelper.PrintResult("사용자(1)의 부서 매니저", manager);

        // 없는 사용자의 경우
        var noManager = FindUser(999)
            .Bind(user => FindDepartment(user.DepartmentId))
            .Bind(dept => FindUser(dept.ManagerId));

        MenuHelper.PrintResult("사용자(999)의 부서 매니저", noManager);

        // ============================================================
        // 5. LINQ 쿼리 구문
        // ============================================================
        MenuHelper.PrintSubHeader("5. LINQ 쿼리 구문");

        MenuHelper.PrintExplanation("LINQ의 from ... select 구문으로 Option을 다룰 수 있습니다.");
        MenuHelper.PrintExplanation("여러 Option을 조합할 때 가독성이 좋아집니다.");
        MenuHelper.PrintBlankLines();

        MenuHelper.PrintCode("// LINQ 쿼리 구문 예제");
        MenuHelper.PrintCode(@"var result = from user in FindUser(1)
              from dept in FindDepartment(user.DepartmentId)
              from mgr in FindUser(dept.ManagerId)
              select $""{user.Name}의 매니저: {mgr.Name}"";");

        var linqResult =
            from user in FindUser(1)
            from dept in FindDepartment(user.DepartmentId)
            from mgr in FindUser(dept.ManagerId)
            select $"{user.Name}의 매니저: {mgr.Name}";

        MenuHelper.PrintResult("LINQ 쿼리 결과", linqResult);

        // ============================================================
        // 6. LINQ로 여러 Option 조합
        // ============================================================
        MenuHelper.PrintSubHeader("6. LINQ로 여러 Option 조합");

        MenuHelper.PrintExplanation("여러 Option 값을 하나로 조합할 수 있습니다.");
        MenuHelper.PrintExplanation("모든 값이 Some이어야 결과도 Some입니다.");
        MenuHelper.PrintBlankLines();

        var firstName = Some("길동");
        var lastName = Some("홍");
        Option<int> age = Some(25);

        var fullInfo =
            from fn in firstName
            from ln in lastName
            from a in age
            select $"{ln}{fn}, {a}세";

        MenuHelper.PrintResult("모든 정보가 있을 때", fullInfo);

        // 하나라도 None이면
        Option<string> middleName = None;
        var withMiddle =
            from fn in firstName
            from mn in middleName
            from ln in lastName
            select $"{ln}{mn}{fn}";

        MenuHelper.PrintResult("중간 이름이 None일 때", withMiddle);

        // ============================================================
        // 7. SelectMany (메서드 구문)
        // ============================================================
        MenuHelper.PrintSubHeader("7. SelectMany (메서드 구문)");

        MenuHelper.PrintExplanation("LINQ의 SelectMany는 Bind와 동일합니다.");
        MenuHelper.PrintExplanation("메서드 체이닝 스타일을 선호하면 SelectMany를 사용합니다.");
        MenuHelper.PrintBlankLines();

        var methodSyntax = firstName
            .SelectMany(fn => lastName, (fn, ln) => $"{ln}{fn}");

        MenuHelper.PrintResult("SelectMany 결과", methodSyntax);

        // ============================================================
        // 8. 실전 예제: 주문 처리
        // ============================================================
        MenuHelper.PrintSubHeader("8. 실전 예제: 주문 처리");

        MenuHelper.PrintExplanation("주문 → 상품 → 재고 확인의 체이닝 예제");
        MenuHelper.PrintBlankLines();

        var orderResult =
            from order in FindOrder(1)
            from product in FindProduct(order.ProductId)
            from stock in CheckStock(product.Id)
            where stock > 0
            select new { Order = order, Product = product, Stock = stock };

        orderResult.Match(
            Some: r => MenuHelper.PrintSuccess($"주문 가능: {r.Product.Name}, 재고: {r.Stock}개"),
            None: () => MenuHelper.PrintError("주문 처리 불가")
        );

        MenuHelper.PrintSuccess("Bind와 LINQ 학습 완료!");
    }

    // ============================================================
    // 헬퍼 타입과 함수들
    // ============================================================

    private record User(int Id, string Name, int DepartmentId);
    private record Department(int Id, string Name, int ManagerId);
    private record Order(int Id, int ProductId, int Quantity);
    private record Product(int Id, string Name, decimal Price);

    private static Option<User> FindUser(int id)
    {
        var users = new Dictionary<int, User>
        {
            { 1, new User(1, "Alice", 10) },
            { 2, new User(2, "Bob", 10) },
            { 3, new User(3, "Charlie", 20) }
        };
        return users.TryGetValue(id, out var user) ? Some(user) : None;
    }

    private static Option<Department> FindDepartment(int id)
    {
        var departments = new Dictionary<int, Department>
        {
            { 10, new Department(10, "Engineering", 2) },
            { 20, new Department(20, "Marketing", 3) }
        };
        return departments.TryGetValue(id, out var dept) ? Some(dept) : None;
    }

    private static Option<Order> FindOrder(int id)
    {
        var orders = new Dictionary<int, Order>
        {
            { 1, new Order(1, 100, 2) },
            { 2, new Order(2, 101, 1) }
        };
        return orders.TryGetValue(id, out var order) ? Some(order) : None;
    }

    private static Option<Product> FindProduct(int id)
    {
        var products = new Dictionary<int, Product>
        {
            { 100, new Product(100, "노트북", 1500000m) },
            { 101, new Product(101, "마우스", 50000m) }
        };
        return products.TryGetValue(id, out var product) ? Some(product) : None;
    }

    private static Option<int> CheckStock(int productId)
    {
        var stock = new Dictionary<int, int>
        {
            { 100, 5 },
            { 101, 0 }
        };
        return stock.TryGetValue(productId, out var qty) ? Some(qty) : None;
    }
}
