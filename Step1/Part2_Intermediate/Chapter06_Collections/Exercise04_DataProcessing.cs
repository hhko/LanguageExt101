using LanguageExt;
using LanguageExt101.Common;
using static LanguageExt.Prelude;

namespace LanguageExt101.Part2_Intermediate.Chapter06_Collections;

/// <summary>
/// 실습 04: 불변 컬렉션을 사용한 데이터 처리
///
/// 학습 목표:
/// - Seq, Map, Set을 활용한 데이터 변환
/// - 함수형 컬렉션 연산 체이닝
/// - 집합 연산을 통한 데이터 분석
///
/// 과제:
/// 1. 판매 데이터 집계
/// 2. 카테고리별 그룹화
/// 3. 고객 세그먼트 분석
/// </summary>
public static class Exercise04_DataProcessing
{
    /// <summary>
    /// 예제를 실행합니다.
    /// </summary>
    public static void Run()
    {
        MenuHelper.PrintHeader("Exercise 04: 데이터 처리");

        // 샘플 데이터
        var products = Seq(
            new Product("P001", "노트북", "전자제품", 1500000m),
            new Product("P002", "마우스", "전자제품", 50000m),
            new Product("P003", "책상", "가구", 300000m),
            new Product("P004", "의자", "가구", 200000m),
            new Product("P005", "모니터", "전자제품", 500000m)
        );

        var orders = Seq(
            new Order("O001", "C001", "P001", 1, DateTime.Parse("2024-01-15")),
            new Order("O002", "C001", "P002", 2, DateTime.Parse("2024-01-16")),
            new Order("O003", "C002", "P003", 1, DateTime.Parse("2024-01-17")),
            new Order("O004", "C002", "P001", 1, DateTime.Parse("2024-01-18")),
            new Order("O005", "C003", "P004", 3, DateTime.Parse("2024-01-19")),
            new Order("O006", "C001", "P005", 1, DateTime.Parse("2024-01-20"))
        );

        var customers = Seq(
            new Customer("C001", "김철수", "VIP"),
            new Customer("C002", "이영희", "일반"),
            new Customer("C003", "박민수", "일반")
        );

        // ============================================================
        // 과제 1: 총 매출 계산
        // ============================================================
        MenuHelper.PrintSubHeader("과제 1: 총 매출 계산");

        MenuHelper.PrintExplanation("모든 주문의 총 매출을 계산하세요.");
        MenuHelper.PrintExplanation("매출 = 상품가격 × 수량");
        MenuHelper.PrintBlankLines();

        // 상품 가격 Map
        var priceMap = products.Fold(
            Map<string, decimal>(),
            (acc, p) => acc.Add(p.ProductId, p.Price)
        );

        var totalRevenue = CalculateTotalRevenue(orders, priceMap);
        MenuHelper.PrintResult("총 매출", $"{totalRevenue:N0}원");

        // ============================================================
        // 과제 2: 카테고리별 매출
        // ============================================================
        MenuHelper.PrintSubHeader("과제 2: 카테고리별 매출");

        MenuHelper.PrintExplanation("카테고리별로 매출을 집계하세요.");
        MenuHelper.PrintBlankLines();

        var categoryMap = products.Fold(
            Map<string, string>(),
            (acc, p) => acc.Add(p.ProductId, p.Category)
        );

        var categoryRevenue = CalculateRevenueByCategory(orders, priceMap, categoryMap);
        foreach (var (category, revenue) in categoryRevenue)
        {
            Console.WriteLine($"  {category}: {revenue:N0}원");
        }

        // ============================================================
        // 과제 3: 고객별 구매 상품
        // ============================================================
        MenuHelper.PrintSubHeader("과제 3: 고객별 구매 상품");

        MenuHelper.PrintExplanation("각 고객이 구매한 상품 목록을 Set으로 만드세요.");
        MenuHelper.PrintBlankLines();

        var productNameMap = products.Fold(
            Map<string, string>(),
            (acc, p) => acc.Add(p.ProductId, p.Name)
        );

        var customerProducts = GetCustomerProducts(orders, productNameMap);
        foreach (var (customerId, productSet) in customerProducts)
        {
            var customerName = customers
                .Find(c => c.CustomerId == customerId)
                .Map(c => c.Name)
                .IfNone("Unknown");
            Console.WriteLine($"  {customerName}: {string.Join(", ", productSet)}");
        }

        // ============================================================
        // 과제 4: VIP 고객 분석
        // ============================================================
        MenuHelper.PrintSubHeader("과제 4: VIP 고객 분석");

        MenuHelper.PrintExplanation("VIP 고객들이 구매한 상품과 일반 고객이 구매한 상품의 차이를 분석하세요.");
        MenuHelper.PrintBlankLines();

        var vipCustomerIds = toSet(customers
            .Filter(c => c.Tier == "VIP")
            .Map(c => c.CustomerId));

        var regularCustomerIds = toSet(customers
            .Filter(c => c.Tier == "일반")
            .Map(c => c.CustomerId));

        var (vipOnlyProducts, regularOnlyProducts, commonProducts) =
            AnalyzeCustomerSegments(orders, vipCustomerIds, regularCustomerIds, productNameMap);

        MenuHelper.PrintResult("VIP만 구매", vipOnlyProducts);
        MenuHelper.PrintResult("일반만 구매", regularOnlyProducts);
        MenuHelper.PrintResult("둘 다 구매", commonProducts);

        // ============================================================
        // 과제 5: 베스트셀러 상품
        // ============================================================
        MenuHelper.PrintSubHeader("과제 5: 베스트셀러 상품");

        MenuHelper.PrintExplanation("가장 많이 팔린 상품 Top 3를 찾으세요.");
        MenuHelper.PrintBlankLines();

        var topProducts = GetTopSellingProducts(orders, productNameMap, 3);
        int rank = 1;
        foreach (var (name, quantity) in topProducts)
        {
            Console.WriteLine($"  {rank++}위: {name} ({quantity}개)");
        }

        MenuHelper.PrintSuccess("실습 04 완료!");
    }

    // ============================================================
    // 데이터 타입
    // ============================================================

    private record Product(string ProductId, string Name, string Category, decimal Price);
    private record Order(string OrderId, string CustomerId, string ProductId, int Quantity, DateTime OrderDate);
    private record Customer(string CustomerId, string Name, string Tier);

    // ============================================================
    // 구현할 함수들
    // ============================================================

    /// <summary>
    /// 총 매출 계산
    /// </summary>
    private static decimal CalculateTotalRevenue(
        Seq<Order> orders,
        Map<string, decimal> priceMap)
    {
        return orders.Fold(0m, (acc, order) =>
            acc + priceMap.Find(order.ProductId).IfNone(0m) * order.Quantity
        );
    }

    /// <summary>
    /// 카테고리별 매출 계산
    /// </summary>
    private static Map<string, decimal> CalculateRevenueByCategory(
        Seq<Order> orders,
        Map<string, decimal> priceMap,
        Map<string, string> categoryMap)
    {
        return orders.Fold(
            Map<string, decimal>(),
            (acc, order) =>
            {
                var category = categoryMap.Find(order.ProductId).IfNone("기타");
                var price = priceMap.Find(order.ProductId).IfNone(0m);
                var revenue = price * order.Quantity;
                var currentTotal = acc.Find(category).IfNone(0m);
                return acc.AddOrUpdate(category, currentTotal + revenue);
            }
        );
    }

    /// <summary>
    /// 고객별 구매 상품 Set
    /// </summary>
    private static Map<string, Set<string>> GetCustomerProducts(
        Seq<Order> orders,
        Map<string, string> productNameMap)
    {
        return orders.Fold(
            Map<string, Set<string>>(),
            (acc, order) =>
            {
                var productName = productNameMap.Find(order.ProductId).IfNone("Unknown");
                var currentSet = acc.Find(order.CustomerId).IfNone(Set<string>());
                return acc.AddOrUpdate(order.CustomerId, currentSet.Add(productName));
            }
        );
    }

    /// <summary>
    /// VIP vs 일반 고객 상품 분석
    /// </summary>
    private static (Set<string> VipOnly, Set<string> RegularOnly, Set<string> Common)
        AnalyzeCustomerSegments(
            Seq<Order> orders,
            Set<string> vipCustomerIds,
            Set<string> regularCustomerIds,
            Map<string, string> productNameMap)
    {
        // VIP 고객이 구매한 상품
        var vipProducts = toSet(orders
            .Filter(o => vipCustomerIds.Contains(o.CustomerId))
            .Map(o => productNameMap.Find(o.ProductId).IfNone("Unknown")));

        // 일반 고객이 구매한 상품
        var regularProducts = toSet(orders
            .Filter(o => regularCustomerIds.Contains(o.CustomerId))
            .Map(o => productNameMap.Find(o.ProductId).IfNone("Unknown")));

        return (
            VipOnly: vipProducts.Except(regularProducts),
            RegularOnly: regularProducts.Except(vipProducts),
            Common: vipProducts.Intersect(regularProducts)
        );
    }

    /// <summary>
    /// 베스트셀러 상품 조회
    /// </summary>
    private static Seq<(string Name, int Quantity)> GetTopSellingProducts(
        Seq<Order> orders,
        Map<string, string> productNameMap,
        int topN)
    {
        // 상품별 판매량 집계
        var salesByProduct = orders.Fold(
            Map<string, int>(),
            (acc, order) =>
            {
                var productName = productNameMap.Find(order.ProductId).IfNone("Unknown");
                var currentQty = acc.Find(productName).IfNone(0);
                return acc.AddOrUpdate(productName, currentQty + order.Quantity);
            }
        );

        // 판매량 기준 정렬 후 Top N
        var sorted = salesByProduct.AsEnumerable()
            .OrderByDescending(kv => kv.Value)
            .Take(topN)
            .Select(kv => (Name: kv.Key, Quantity: kv.Value))
            .ToArray();
        return toSeq(sorted);
    }
}
