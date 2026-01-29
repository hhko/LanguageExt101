using LanguageExt;
using LanguageExt.Traits;
using LanguageExt101.Common;
using static LanguageExt.Prelude;

namespace LanguageExt101.Part3_Advanced.Chapter12_Traits;

/// <summary>
/// Monad와 Alternative 트레이트를 상세히 학습합니다.
///
/// 학습 목표:
/// - Monad&lt;M&gt; 트레이트와 Bind 연산 마스터
/// - Monad Laws 이해 및 검증
/// - LINQ와 Monad의 관계 (from-select)
/// - Alternative&lt;M&gt; 트레이트와 Choose 연산
/// - 폴백(Fallback) 패턴 구현
///
/// 핵심 개념:
/// Monad는 "순차적 의존성을 가진 연산을 체이닝"합니다.
/// Alternative는 "실패 시 대안을 선택"하는 능력을 제공합니다.
/// </summary>
public static class E03_MonadAlternative
{
    public static void Run()
    {
        MenuHelper.PrintHeader("Chapter 12-E03: Monad와 Alternative");

        // ============================================================
        // 1. Monad: Bind 연산
        // ============================================================
        MenuHelper.PrintSubHeader("1. Monad: Bind 연산");

        MenuHelper.PrintExplanation("Monad의 핵심은 Bind(FlatMap)입니다:");
        MenuHelper.PrintExplanation("이전 값을 사용해 다음 모나드를 생성하고 평탄화합니다.");
        MenuHelper.PrintBlankLines();

        MenuHelper.PrintCode("// Bind의 시그니처");
        MenuHelper.PrintCode("// Bind<A, B>(M<A>, Func<A, M<B>>) -> M<B>");
        MenuHelper.PrintBlankLines();

        // 기본 Bind 예시
        var bindResult = Some(5).Bind(x => Some(x * 2));
        MenuHelper.PrintCode("Some(5).Bind(x => Some(x * 2))");
        MenuHelper.PrintResult("결과", bindResult);

        // None에서 Bind
        var bindNone = Option<int>.None.Bind(x => Some(x * 2));
        MenuHelper.PrintCode("Option<int>.None.Bind(x => Some(x * 2))");
        MenuHelper.PrintResult("None의 Bind", bindNone);

        // Bind 체이닝
        var chain = Some(10)
            .Bind(x => Some(x + 5))
            .Bind(x => Some(x * 2))
            .Bind(x => Some($"결과: {x}"));
        MenuHelper.PrintCode("Some(10).Bind(x => Some(x+5)).Bind(x => Some(x*2)).Bind(...)");
        MenuHelper.PrintResult("체인 결과", chain); // (10 + 5) * 2 = 30

        // ============================================================
        // 2. Monad vs Applicative
        // ============================================================
        MenuHelper.PrintSubHeader("2. Monad vs Applicative");

        MenuHelper.PrintExplanation("핵심 차이: Monad는 이전 값에 의존하는 연산이 가능합니다.");
        MenuHelper.PrintBlankLines();

        MenuHelper.PrintCode("// Applicative: 값들이 서로 독립적");
        MenuHelper.PrintCode("// a와 b를 동시에 평가 가능");
        MenuHelper.PrintCode("getA().Map(curry(f)).Apply(getB())");
        MenuHelper.PrintBlankLines();

        MenuHelper.PrintCode("// Monad: 다음 연산이 이전 값에 의존");
        MenuHelper.PrintCode("// a를 먼저 얻어야 b를 계산 가능");
        MenuHelper.PrintCode("getA().Bind(a => getB(a))");
        MenuHelper.PrintBlankLines();

        // 의존적 연산 예시
        MenuHelper.PrintExplanation("예: 사용자 ID로 사용자 조회 -> 그 사용자의 주문 조회");
        var userOrders =
            from user in FindUser(1)
            from orders in GetOrders(user.Id)
            select (user.Name, orders);
        MenuHelper.PrintResult("사용자와 주문", userOrders);

        // ============================================================
        // 3. Monad Laws
        // ============================================================
        MenuHelper.PrintSubHeader("3. Monad Laws");

        MenuHelper.PrintExplanation("Monad는 세 가지 법칙을 만족해야 합니다:");
        MenuHelper.PrintBlankLines();

        MenuHelper.PrintCode("// 1. Left Identity:  pure(a).Bind(f) == f(a)");
        MenuHelper.PrintCode("// 2. Right Identity: m.Bind(pure) == m");
        MenuHelper.PrintCode("// 3. Associativity:  m.Bind(f).Bind(g) == m.Bind(x => f(x).Bind(g))");
        MenuHelper.PrintBlankLines();

        int testVal = 5;
        Func<int, Option<int>> monadF = x => Some(x * 2);
        Func<int, Option<int>> monadG = x => Some(x + 10);

        // Left Identity
        var left1 = Some(testVal).Bind(monadF);
        var left2 = monadF(testVal);
        MenuHelper.PrintResult("Left Identity 성립", left1.Equals(left2));

        // Right Identity
        var right1 = Some(testVal).Bind(x => Some(x));
        var right2 = Some(testVal);
        MenuHelper.PrintResult("Right Identity 성립", right1.Equals(right2));

        // Associativity
        var assoc1 = Some(testVal).Bind(monadF).Bind(monadG);
        var assoc2 = Some(testVal).Bind(x => monadF(x).Bind(monadG));
        MenuHelper.PrintResult("Associativity 성립", assoc1.Equals(assoc2));

        // ============================================================
        // 4. LINQ와 Monad
        // ============================================================
        MenuHelper.PrintSubHeader("4. LINQ와 Monad");

        MenuHelper.PrintExplanation("C#의 LINQ 쿼리 구문은 Monad의 do-notation입니다.");
        MenuHelper.PrintExplanation("from-select는 Bind-Map으로 변환됩니다.");
        MenuHelper.PrintBlankLines();

        // LINQ 사용
        var linqResult =
            from x in Some(5)
            from y in Some(x * 2)
            from z in Some(y + 10)
            select $"x={x}, y={y}, z={z}";

        MenuHelper.PrintCode("from x in Some(5)");
        MenuHelper.PrintCode("from y in Some(x * 2)  // y는 x에 의존");
        MenuHelper.PrintCode("from z in Some(y + 10) // z는 y에 의존");
        MenuHelper.PrintCode("select $\"x={x}, y={y}, z={z}\"");
        MenuHelper.PrintResult("LINQ 결과", linqResult);

        // 동등한 Bind 체인
        MenuHelper.PrintCode("// 위는 아래와 동등");
        MenuHelper.PrintCode("Some(5).Bind(x => Some(x*2).Bind(y => Some(y+10).Map(z => ...)))");

        // LINQ에서 조기 실패
        var linqFailed =
            from x in Some(10)
            from y in Option<int>.None  // 여기서 실패
            from z in Some(x + y)
            select z;
        MenuHelper.PrintResult("중간에 None 발생", linqFailed);

        // ============================================================
        // 5. 제네릭 Monad 함수
        // ============================================================
        MenuHelper.PrintSubHeader("5. 제네릭 Monad 함수");

        MenuHelper.PrintExplanation("Monad<M> 제약으로 순차적 체이닝 로직을 범용화합니다.");
        MenuHelper.PrintBlankLines();

        MenuHelper.PrintCode("// 조건부 실행");
        MenuHelper.PrintCode("static K<M, A> When<M, A>(K<M, A> ma, Func<A, bool> pred, Func<A, K<M, A>> f)");
        MenuHelper.PrintCode("    where M : Monad<M>");
        MenuHelper.PrintCode("    => ma.Bind(a => pred(a) ? f(a) : M.Pure(a));");
        MenuHelper.PrintBlankLines();

        var whenResult = When<Option, int>(Some(10), x => x > 5, x => Some(x * 100));
        var whenFalse = When<Option, int>(Some(3), x => x > 5, x => Some(x * 100));
        MenuHelper.PrintResult("When(Some(10), >5, *100)", whenResult.As());
        MenuHelper.PrintResult("When(Some(3), >5, *100)", whenFalse.As());

        // ============================================================
        // 6. Alternative: Choose 연산
        // ============================================================
        MenuHelper.PrintSubHeader("6. Alternative: Choose 연산");

        MenuHelper.PrintExplanation("Alternative는 '실패 시 대안 선택' 능력을 제공합니다.");
        MenuHelper.PrintExplanation("Choose: 첫 번째가 실패하면 두 번째를 사용합니다.");
        MenuHelper.PrintBlankLines();

        MenuHelper.PrintCode("// Choose의 시그니처");
        MenuHelper.PrintCode("// Choose<A>(M<A>, M<A>) -> M<A>");
        MenuHelper.PrintBlankLines();

        var primary = Option<int>.None;
        var fallback = Some(42);
        var chosen = primary.Choose(fallback);
        MenuHelper.PrintCode("Option<int>.None.Choose(Some(42))");
        MenuHelper.PrintResult("폴백 적용", chosen);

        var noPrimaryNeeded = Some(100).Choose(Some(42));
        MenuHelper.PrintResult("Some(100)은 폴백 불필요", noPrimaryNeeded);

        // | 연산자
        MenuHelper.PrintBlankLines();
        MenuHelper.PrintCode("// | 연산자로도 Choose 가능");
        var orResult = Option<int>.None | Some(10) | Some(20);
        MenuHelper.PrintCode("None | Some(10) | Some(20)");
        MenuHelper.PrintResult("첫 성공", orResult);

        // ============================================================
        // 7. 폴백 패턴
        // ============================================================
        MenuHelper.PrintSubHeader("7. 폴백 패턴");

        MenuHelper.PrintExplanation("여러 소스에서 순차적으로 값을 찾는 패턴입니다.");
        MenuHelper.PrintBlankLines();

        MenuHelper.PrintCode("// 환경변수 -> 설정파일 -> 기본값 순으로 찾기");
        var config = GetEnvConfig("DB_HOST")
            | GetFileConfig("database.host")
            | Some("localhost");
        MenuHelper.PrintResult("설정값", config);

        // 제네릭 폴백 함수
        MenuHelper.PrintBlankLines();
        MenuHelper.PrintCode("// 제네릭 폴백: 여러 대안 중 첫 성공");
        var firstOf = FirstSuccess<Option, int>(
            Option<int>.None,
            Option<int>.None,
            Some(100),
            Some(200)
        );
        MenuHelper.PrintResult("FirstSuccess", firstOf.As());

        // ============================================================
        // 8. Monad + Alternative 조합
        // ============================================================
        MenuHelper.PrintSubHeader("8. Monad + Alternative 조합");

        MenuHelper.PrintExplanation("Monad와 Alternative를 함께 사용하면 강력합니다.");
        MenuHelper.PrintExplanation("순차 처리 + 실패 시 대안 패턴");
        MenuHelper.PrintBlankLines();

        MenuHelper.PrintCode("// 파이프라인 with 폴백");
        var pipeline =
            (from user in FindUser(999)  // 존재하지 않는 ID
             from orders in GetOrders(user.Id)
             select orders)
            | Some(Seq<string>());  // 실패 시 빈 목록

        MenuHelper.PrintResult("없는 사용자 주문 (폴백)", pipeline);

        // ============================================================
        // 9. 실전 예제: 로그인 파이프라인
        // ============================================================
        MenuHelper.PrintSubHeader("9. 실전 예제: 로그인 파이프라인");

        MenuHelper.PrintExplanation("사용자 인증 파이프라인 예제입니다.");
        MenuHelper.PrintExplanation("각 단계가 이전 단계의 결과에 의존합니다.");
        MenuHelper.PrintBlankLines();

        var loginSuccess = Login("john@example.com", "password123");
        var loginFailed = Login("unknown@example.com", "password123");
        var loginWrongPw = Login("john@example.com", "wrongpw");

        MenuHelper.PrintCode("Login(\"john@example.com\", \"password123\")");
        MenuHelper.PrintResult("성공", loginSuccess);

        MenuHelper.PrintCode("Login(\"unknown@example.com\", \"password123\")");
        MenuHelper.PrintResult("없는 사용자", loginFailed);

        MenuHelper.PrintCode("Login(\"john@example.com\", \"wrongpw\")");
        MenuHelper.PrintResult("잘못된 비밀번호", loginWrongPw);

        // ============================================================
        // 10. 정리
        // ============================================================
        MenuHelper.PrintSubHeader("10. 정리");

        MenuHelper.PrintExplanation("Monad<M>:");
        MenuHelper.PrintExplanation("  - Bind로 순차적 의존 연산 체이닝");
        MenuHelper.PrintExplanation("  - LINQ from-select 구문 지원");
        MenuHelper.PrintExplanation("  - Applicative보다 강력하지만 제약도 강함");
        MenuHelper.PrintBlankLines();

        MenuHelper.PrintExplanation("Alternative<M>:");
        MenuHelper.PrintExplanation("  - Choose로 실패 시 대안 선택");
        MenuHelper.PrintExplanation("  - | 연산자로 간편하게 사용");
        MenuHelper.PrintExplanation("  - 폴백 패턴 구현에 유용");
        MenuHelper.PrintBlankLines();

        MenuHelper.PrintExplanation("다음: E04에서 Foldable과 Traversable을 학습합니다.");

        MenuHelper.PrintSuccess("Monad와 Alternative 학습 완료!");
    }

    // ============================================================
    // 헬퍼 함수들
    // ============================================================

    /// <summary>
    /// 조건부 실행 범용 함수 (Monad)
    /// </summary>
    private static K<M, A> When<M, A>(K<M, A> ma, Func<A, bool> pred, Func<A, K<M, A>> f)
        where M : Monad<M>
        => ma.Bind(a => pred(a) ? f(a) : M.Pure(a));

    /// <summary>
    /// 여러 대안 중 첫 성공 선택 (Alternative)
    /// </summary>
    private static K<M, A> FirstSuccess<M, A>(params K<M, A>[] alternatives)
        where M : Alternative<M>
    {
        K<M, A> result = M.Empty<A>();
        foreach (var alt in alternatives)
        {
            result = result.Choose(alt);
        }
        return result;
    }

    // ============================================================
    // 도메인 모델
    // ============================================================

    private record User(int Id, string Name, string Email);

    private static Option<User> FindUser(int id)
        => id switch
        {
            1 => Some(new User(1, "John", "john@example.com")),
            2 => Some(new User(2, "Jane", "jane@example.com")),
            _ => None
        };

    private static Option<Seq<string>> GetOrders(int userId)
        => userId switch
        {
            1 => Some(Seq("Laptop", "Mouse", "Keyboard")),
            2 => Some(Seq("Monitor")),
            _ => None
        };

    private static Option<string> GetEnvConfig(string key)
        => None; // 환경변수에 없다고 가정

    private static Option<string> GetFileConfig(string key)
        => None; // 파일에도 없다고 가정

    private static Option<User> FindUserByEmail(string email)
        => email switch
        {
            "john@example.com" => Some(new User(1, "John", email)),
            "jane@example.com" => Some(new User(2, "Jane", email)),
            _ => None
        };

    private static Option<User> ValidatePassword(User user, string password)
        => password == "password123" ? Some(user) : None;

    private static Option<string> GenerateToken(User user)
        => Some($"token_{user.Id}_{DateTime.Now.Ticks}");

    private static Option<string> Login(string email, string password)
    {
        return from user in FindUserByEmail(email)
               from valid in ValidatePassword(user, password)
               from token in GenerateToken(valid)
               select token;
    }
}
