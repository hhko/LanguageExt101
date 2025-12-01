using LanguageExt;
using LanguageExt.Common;
using LanguageExt101.Common;
using static LanguageExt.Prelude;

namespace LanguageExt101.Part2_Intermediate.Chapter07_Validation;

/// <summary>
/// 실무에서의 Validation 활용을 학습합니다.
///
/// 학습 목표:
/// - 도메인 객체 생성 검증
/// - 커스텀 에러 타입
/// - 검증 규칙 재사용
///
/// 핵심 개념:
/// 실무에서 Validation은 도메인 객체 생성 시점에 주로 사용됩니다.
/// "유효하지 않은 상태를 표현할 수 없는" 타입 설계와 함께 사용하면 강력합니다.
/// </summary>
public static class E03_RealWorldValidation
{
    /// <summary>
    /// 예제를 실행합니다.
    /// </summary>
    public static void Run()
    {
        MenuHelper.PrintHeader("Chapter 07-E03: 실무 검증 예제");

        // ============================================================
        // 1. 도메인 객체 생성 검증
        // ============================================================
        MenuHelper.PrintSubHeader("1. 도메인 객체 생성 검증");

        MenuHelper.PrintExplanation("유효한 값만 담을 수 있는 도메인 객체를 만듭니다.");
        MenuHelper.PrintExplanation("생성 시점에 검증을 수행합니다.");
        MenuHelper.PrintBlankLines();

        // Email 값 객체 생성
        var validEmail = Email.Create("user@example.com");
        var invalidEmail = Email.Create("invalid-email");

        MenuHelper.PrintResult("유효한 이메일", validEmail);
        MenuHelper.PrintResult("무효한 이메일", invalidEmail);

        // ============================================================
        // 2. 복합 도메인 객체
        // ============================================================
        MenuHelper.PrintSubHeader("2. 복합 도메인 객체");

        MenuHelper.PrintExplanation("여러 값 객체를 조합하여 복합 도메인 객체를 생성합니다.");
        MenuHelper.PrintBlankLines();

        var validUser = CreateUser("Alice", "alice@example.com", 25);
        var invalidUser = CreateUser("A", "bad-email", -5);

        validUser.Match(
            Succ: user => MenuHelper.PrintSuccess($"유효한 사용자: {user}"),
            Fail: error => MenuHelper.PrintError($"검증 실패: {error.Message}")
        );

        invalidUser.Match(
            Succ: user => MenuHelper.PrintSuccess($"유효한 사용자: {user}"),
            Fail: error => MenuHelper.PrintError($"검증 실패: {error.Message}")
        );

        // ============================================================
        // 3. 조건부 검증
        // ============================================================
        MenuHelper.PrintSubHeader("3. 조건부 검증");

        MenuHelper.PrintExplanation("다른 필드 값에 따라 검증 규칙이 달라지는 경우입니다.");
        MenuHelper.PrintBlankLines();

        // 개인 회원
        var personalAccount = CreateAccount(
            accountType: "personal",
            name: "홍길동",
            companyName: null,
            taxId: null
        );

        // 기업 회원 (회사명, 사업자번호 필수)
        var businessAccount = CreateAccount(
            accountType: "business",
            name: "김대표",
            companyName: "테크회사",
            taxId: "123-45-67890"
        );

        // 기업인데 회사정보 없음
        var invalidBusiness = CreateAccount(
            accountType: "business",
            name: "박대표",
            companyName: null,  // 필수인데 없음
            taxId: null  // 필수인데 없음
        );

        personalAccount.Match(
            Succ: a => MenuHelper.PrintSuccess($"개인 계정: {a.Name}"),
            Fail: error => MenuHelper.PrintError($"검증 실패: {error.Message}")
        );

        businessAccount.Match(
            Succ: a => MenuHelper.PrintSuccess($"기업 계정: {a.Name} ({a.CompanyName})"),
            Fail: error => MenuHelper.PrintError($"검증 실패: {error.Message}")
        );

        invalidBusiness.Match(
            Succ: a => MenuHelper.PrintSuccess($"계정: {a.Name}"),
            Fail: error => MenuHelper.PrintError($"검증 실패: {error.Message}")
        );

        // ============================================================
        // 4. 검증 규칙 재사용
        // ============================================================
        MenuHelper.PrintSubHeader("4. 검증 규칙 재사용");

        MenuHelper.PrintExplanation("공통 검증 규칙을 재사용합니다.");
        MenuHelper.PrintBlankLines();

        // 재사용 가능한 검증기
        var shortText = ValidateLength("이름", "A", 2, 50);
        var longText = ValidateLength("설명", new string('a', 100), 1, 50);
        var validText = ValidateLength("제목", "적절한 길이", 1, 50);

        MenuHelper.PrintResult("너무 짧은 텍스트", shortText);
        MenuHelper.PrintResult("너무 긴 텍스트", longText);
        MenuHelper.PrintResult("적절한 텍스트", validText);

        // ============================================================
        // 5. 중첩 객체 검증
        // ============================================================
        MenuHelper.PrintSubHeader("5. 중첩 객체 검증");

        MenuHelper.PrintExplanation("중첩된 객체 구조를 검증합니다.");
        MenuHelper.PrintBlankLines();

        var validOrder = ValidateOrder(
            customerId: "C001",
            items: Seq(
                ("P001", 2),
                ("P002", 1)
            ),
            shippingAddress: "서울시 강남구"
        );

        var invalidOrder = ValidateOrder(
            customerId: "",
            items: Seq<(string, int)>(),  // 빈 주문
            shippingAddress: ""
        );

        validOrder.Match(
            Succ: o => MenuHelper.PrintSuccess($"유효한 주문: 고객 {o.CustomerId}, {o.Items.Count}개 항목"),
            Fail: error => MenuHelper.PrintError($"검증 실패: {error.Message}")
        );

        invalidOrder.Match(
            Succ: o => MenuHelper.PrintSuccess($"유효한 주문: {o.CustomerId}"),
            Fail: error => MenuHelper.PrintError($"검증 실패: {error.Message}")
        );

        MenuHelper.PrintSuccess("실무 검증 예제 학습 완료!");
    }

    // ============================================================
    // 값 객체 (Value Objects)
    // ============================================================

    public record Email
    {
        public string Value { get; }

        private Email(string value) => Value = value;

        public static Validation<Error, Email> Create(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                return Fail<Error, Email>(Error.New("이메일은 필수입니다"));
            if (!email.Contains('@'))
                return Fail<Error, Email>(Error.New("올바른 이메일 형식이 아닙니다"));
            if (email.Length > 255)
                return Fail<Error, Email>(Error.New("이메일이 너무 깁니다"));

            return Success<Error, Email>(new Email(email));
        }

        public override string ToString() => Value;
    }

    public record UserName
    {
        public string Value { get; }

        private UserName(string value) => Value = value;

        public static Validation<Error, UserName> Create(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                return Fail<Error, UserName>(Error.New("이름은 필수입니다"));
            if (name.Length < 2)
                return Fail<Error, UserName>(Error.New("이름은 2자 이상이어야 합니다"));
            if (name.Length > 50)
                return Fail<Error, UserName>(Error.New("이름은 50자 이하여야 합니다"));

            return Success<Error, UserName>(new UserName(name));
        }

        public override string ToString() => Value;
    }

    public record Age
    {
        public int Value { get; }

        private Age(int value) => Value = value;

        public static Validation<Error, Age> Create(int age)
        {
            if (age < 0)
                return Fail<Error, Age>(Error.New("나이는 0 이상이어야 합니다"));
            if (age > 150)
                return Fail<Error, Age>(Error.New("나이는 150 이하여야 합니다"));

            return Success<Error, Age>(new Age(age));
        }

        public override string ToString() => Value.ToString();
    }

    // ============================================================
    // 복합 도메인 객체
    // ============================================================

    public record ValidatedUser(UserName Name, Email Email, Age Age)
    {
        public override string ToString() => $"{Name.Value} ({Email.Value}), {Age.Value}세";
    }

    private static Validation<Error, ValidatedUser> CreateUser(string name, string email, int age)
    {
        var nameV = UserName.Create(name);
        var emailV = Email.Create(email);
        var ageV = Age.Create(age);

        Error? errors = null;
        nameV.IfFail(e => errors = errors == null ? e : errors + e);
        emailV.IfFail(e => errors = errors == null ? e : errors + e);
        ageV.IfFail(e => errors = errors == null ? e : errors + e);

        if (errors == null)
        {
            return Success<Error, ValidatedUser>(new ValidatedUser(
                nameV.Match(Succ: n => n, Fail: _ => throw new InvalidOperationException()),
                emailV.Match(Succ: e => e, Fail: _ => throw new InvalidOperationException()),
                ageV.Match(Succ: a => a, Fail: _ => throw new InvalidOperationException())
            ));
        }

        return Fail<Error, ValidatedUser>(errors);
    }

    // ============================================================
    // 조건부 검증
    // ============================================================

    public record Account(string AccountType, string Name, string? CompanyName, string? TaxId);

    private static Validation<Error, Account> CreateAccount(
        string accountType,
        string name,
        string? companyName,
        string? taxId)
    {
        Error? errors = null;

        // 공통 검증
        if (string.IsNullOrWhiteSpace(name))
            errors = errors == null ? Error.New("이름은 필수입니다") : errors + Error.New("이름은 필수입니다");

        // 기업 계정인 경우 추가 검증
        if (accountType == "business")
        {
            if (string.IsNullOrWhiteSpace(companyName))
                errors = errors == null ? Error.New("기업 계정은 회사명이 필수입니다") : errors + Error.New("기업 계정은 회사명이 필수입니다");

            if (string.IsNullOrWhiteSpace(taxId))
                errors = errors == null ? Error.New("기업 계정은 사업자번호가 필수입니다") : errors + Error.New("기업 계정은 사업자번호가 필수입니다");
        }

        return errors == null
            ? Success<Error, Account>(new Account(accountType, name, companyName, taxId))
            : Fail<Error, Account>(errors);
    }

    // ============================================================
    // 재사용 가능한 검증기
    // ============================================================

    private static Validation<Error, string> ValidateLength(
        string fieldName,
        string value,
        int minLength,
        int maxLength)
    {
        if (value.Length < minLength)
            return Fail<Error, string>(Error.New($"{fieldName}은(는) {minLength}자 이상이어야 합니다"));

        if (value.Length > maxLength)
            return Fail<Error, string>(Error.New($"{fieldName}은(는) {maxLength}자 이하여야 합니다"));

        return Success<Error, string>(value);
    }

    // ============================================================
    // 중첩 객체 검증
    // ============================================================

    public record OrderItem(string ProductId, int Quantity);
    public record Order(string CustomerId, Seq<OrderItem> Items, string ShippingAddress);

    private static Validation<Error, Order> ValidateOrder(
        string customerId,
        Seq<(string productId, int quantity)> items,
        string shippingAddress)
    {
        Error? errors = null;

        // 고객 ID 검증
        if (string.IsNullOrWhiteSpace(customerId))
            errors = errors == null ? Error.New("고객 ID는 필수입니다") : errors + Error.New("고객 ID는 필수입니다");

        // 주문 항목 검증
        if (items.IsEmpty)
            errors = errors == null ? Error.New("최소 1개 이상의 상품이 필요합니다") : errors + Error.New("최소 1개 이상의 상품이 필요합니다");

        foreach (var (productId, quantity) in items)
        {
            if (string.IsNullOrWhiteSpace(productId))
                errors = errors == null ? Error.New("상품 ID는 필수입니다") : errors + Error.New("상품 ID는 필수입니다");
            if (quantity <= 0)
            {
                var msg = $"수량은 1 이상이어야 합니다 (상품: {productId})";
                errors = errors == null ? Error.New(msg) : errors + Error.New(msg);
            }
        }

        // 배송 주소 검증
        if (string.IsNullOrWhiteSpace(shippingAddress))
            errors = errors == null ? Error.New("배송 주소는 필수입니다") : errors + Error.New("배송 주소는 필수입니다");

        if (errors == null)
        {
            var orderItems = items.Map(i => new OrderItem(i.productId, i.quantity));
            return Success<Error, Order>(new Order(customerId, orderItems, shippingAddress));
        }

        return Fail<Error, Order>(errors);
    }
}
