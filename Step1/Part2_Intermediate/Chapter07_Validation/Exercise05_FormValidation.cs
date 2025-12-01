using LanguageExt;
using LanguageExt.Common;
using LanguageExt101.Common;
using static LanguageExt.Prelude;

namespace LanguageExt101.Part2_Intermediate.Chapter07_Validation;

/// <summary>
/// 실습 05: 종합 폼 검증 시스템
///
/// 학습 목표:
/// - Validation을 활용한 폼 검증
/// - 여러 에러 수집
/// - 실무적인 검증 패턴
///
/// 과제:
/// 주문 폼 검증 시스템을 구현합니다.
/// </summary>
public static class Exercise05_FormValidation
{
    /// <summary>
    /// 예제를 실행합니다.
    /// </summary>
    public static void Run()
    {
        MenuHelper.PrintHeader("Exercise 05: 폼 검증");

        // ============================================================
        // 과제: 주문 폼 검증
        // ============================================================
        MenuHelper.PrintSubHeader("주문 폼 검증 시스템");

        MenuHelper.PrintExplanation("다음 필드들을 검증하세요:");
        MenuHelper.PrintExplanation("- 고객명: 필수, 2-50자");
        MenuHelper.PrintExplanation("- 전화번호: 필수, 010-XXXX-XXXX 형식");
        MenuHelper.PrintExplanation("- 주소: 필수, 10자 이상");
        MenuHelper.PrintExplanation("- 상품: 최소 1개, 수량 1-100");
        MenuHelper.PrintExplanation("- 결제수단: card 또는 bank_transfer");
        MenuHelper.PrintBlankLines();

        // 테스트 케이스 1: 모두 유효
        var valid = ValidateOrderForm(
            customerName: "홍길동",
            phone: "010-1234-5678",
            address: "서울시 강남구 테헤란로 123번지 빌딩 10층",
            items: Seq(("상품A", 2), ("상품B", 1)),
            paymentMethod: "card"
        );

        Console.WriteLine("테스트 1: 유효한 주문");
        PrintResult(valid);
        MenuHelper.PrintBlankLines();

        // 테스트 케이스 2: 여러 오류
        var invalid = ValidateOrderForm(
            customerName: "A",
            phone: "1234567890",
            address: "짧은주소",
            items: Seq<(string, int)>(),
            paymentMethod: "cash"
        );

        Console.WriteLine("테스트 2: 여러 오류가 있는 주문");
        PrintResult(invalid);
        MenuHelper.PrintBlankLines();

        // 테스트 케이스 3: 일부 오류
        var partial = ValidateOrderForm(
            customerName: "김철수",
            phone: "010-1234-5678",
            address: "서울시 강남구",  // 너무 짧음
            items: Seq(("상품A", 150)),  // 수량 초과
            paymentMethod: "card"
        );

        Console.WriteLine("테스트 3: 일부 오류가 있는 주문");
        PrintResult(partial);

        MenuHelper.PrintSuccess("실습 05 완료!");
    }

    // ============================================================
    // 데이터 타입
    // ============================================================

    public record OrderItem(string ProductName, int Quantity);

    public record OrderForm(
        string CustomerName,
        string Phone,
        string Address,
        Seq<OrderItem> Items,
        string PaymentMethod);

    // ============================================================
    // 구현할 함수들
    // ============================================================

    /// <summary>
    /// 고객명 검증
    /// </summary>
    private static Validation<Error, string> ValidateCustomerName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            return Fail<Error, string>(Error.New("[고객명] 필수 입력입니다"));
        if (name.Length < 2)
            return Fail<Error, string>(Error.New("[고객명] 2자 이상이어야 합니다"));
        if (name.Length > 50)
            return Fail<Error, string>(Error.New("[고객명] 50자 이하여야 합니다"));

        return Success<Error, string>(name);
    }

    /// <summary>
    /// 전화번호 검증
    /// </summary>
    private static Validation<Error, string> ValidatePhone(string phone)
    {
        if (string.IsNullOrWhiteSpace(phone))
            return Fail<Error, string>(Error.New("[전화번호] 필수 입력입니다"));
        if (!System.Text.RegularExpressions.Regex.IsMatch(phone, @"^010-\d{4}-\d{4}$"))
            return Fail<Error, string>(Error.New("[전화번호] 010-XXXX-XXXX 형식이어야 합니다"));

        return Success<Error, string>(phone);
    }

    /// <summary>
    /// 주소 검증
    /// </summary>
    private static Validation<Error, string> ValidateAddress(string address)
    {
        if (string.IsNullOrWhiteSpace(address))
            return Fail<Error, string>(Error.New("[주소] 필수 입력입니다"));
        if (address.Length < 10)
            return Fail<Error, string>(Error.New("[주소] 10자 이상 입력해주세요"));

        return Success<Error, string>(address);
    }

    /// <summary>
    /// 상품 목록 검증
    /// </summary>
    private static Validation<Error, Seq<OrderItem>> ValidateItems(Seq<(string name, int qty)> items)
    {
        Error? errors = null;

        if (items.IsEmpty)
        {
            return Fail<Error, Seq<OrderItem>>(Error.New("[상품] 최소 1개 이상 선택해주세요"));
        }

        int idx = 1;
        foreach (var (name, qty) in items)
        {
            if (string.IsNullOrWhiteSpace(name))
                errors = errors == null ? Error.New($"[상품 {idx}] 상품명이 비어있습니다") : errors + Error.New($"[상품 {idx}] 상품명이 비어있습니다");

            if (qty < 1)
                errors = errors == null ? Error.New($"[상품 {idx}] 수량은 1 이상이어야 합니다") : errors + Error.New($"[상품 {idx}] 수량은 1 이상이어야 합니다");
            else if (qty > 100)
                errors = errors == null ? Error.New($"[상품 {idx}] 수량은 100 이하여야 합니다") : errors + Error.New($"[상품 {idx}] 수량은 100 이하여야 합니다");

            idx++;
        }

        return errors == null
            ? Success<Error, Seq<OrderItem>>(items.Map(i => new OrderItem(i.name, i.qty)))
            : Fail<Error, Seq<OrderItem>>(errors);
    }

    /// <summary>
    /// 결제수단 검증
    /// </summary>
    private static Validation<Error, string> ValidatePaymentMethod(string method)
    {
        var validMethods = Set("card", "bank_transfer");

        if (!validMethods.Contains(method))
            return Fail<Error, string>(Error.New("[결제수단] card 또는 bank_transfer만 가능합니다"));

        return Success<Error, string>(method);
    }

    /// <summary>
    /// 전체 폼 검증
    /// </summary>
    private static Validation<Error, OrderForm> ValidateOrderForm(
        string customerName,
        string phone,
        string address,
        Seq<(string, int)> items,
        string paymentMethod)
    {
        var nameV = ValidateCustomerName(customerName);
        var phoneV = ValidatePhone(phone);
        var addressV = ValidateAddress(address);
        var itemsV = ValidateItems(items);
        var paymentV = ValidatePaymentMethod(paymentMethod);

        // 모든 에러 수집
        Error? errors = null;
        nameV.IfFail(e => errors = errors == null ? e : errors + e);
        phoneV.IfFail(e => errors = errors == null ? e : errors + e);
        addressV.IfFail(e => errors = errors == null ? e : errors + e);
        itemsV.IfFail(e => errors = errors == null ? e : errors + e);
        paymentV.IfFail(e => errors = errors == null ? e : errors + e);

        if (errors == null)
        {
            return Success<Error, OrderForm>(new OrderForm(
                CustomerName: nameV.Match(Succ: n => n, Fail: _ => ""),
                Phone: phoneV.Match(Succ: p => p, Fail: _ => ""),
                Address: addressV.Match(Succ: a => a, Fail: _ => ""),
                Items: itemsV.Match(Succ: i => i, Fail: _ => Seq<OrderItem>()),
                PaymentMethod: paymentV.Match(Succ: m => m, Fail: _ => "")
            ));
        }

        return Fail<Error, OrderForm>(errors);
    }

    /// <summary>
    /// 결과 출력
    /// </summary>
    private static void PrintResult(Validation<Error, OrderForm> result)
    {
        result.Match(
            Succ: form =>
            {
                MenuHelper.PrintSuccess("검증 성공!");
                Console.WriteLine($"  고객명: {form.CustomerName}");
                Console.WriteLine($"  전화번호: {form.Phone}");
                Console.WriteLine($"  주소: {form.Address}");
                Console.WriteLine($"  상품: {form.Items.Count}개");
                foreach (var item in form.Items)
                {
                    Console.WriteLine($"    - {item.ProductName} x {item.Quantity}");
                }
                Console.WriteLine($"  결제: {form.PaymentMethod}");
            },
            Fail: error => MenuHelper.PrintError($"검증 실패: {error.Message}")
        );
    }
}
