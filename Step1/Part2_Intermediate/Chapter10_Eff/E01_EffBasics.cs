using LanguageExt;
using LanguageExt.Common;
using LanguageExt101.Common;
using static LanguageExt.Prelude;

namespace LanguageExt101.Part2_Intermediate.Chapter10_Eff;

/// <summary>
/// Eff 모나드의 기본 사용법을 학습합니다.
///
/// 학습 목표:
/// - Eff&lt;A&gt;와 IO&lt;A&gt;의 차이점 이해
/// - Eff 기본 생성 (Pure, Fail, lift)
/// - Eff 실행 방법 (Run, RunIO)
/// - LINQ 합성
/// - guard 패턴
///
/// 핵심 개념:
/// Eff&lt;A&gt;는 IO&lt;A&gt;와 비슷하지만 더 풍부한 에러 처리를 제공합니다.
/// - 실패를 Error 타입으로 추적
/// - 에러 복구 연산자 (@catch) 내장
/// - guard를 통한 조건부 실패
/// - Runtime을 통한 의존성 주입 지원 (Eff&lt;RT, A&gt;)
/// </summary>
public static class E01_EffBasics
{
    public static void Run()
    {
        MenuHelper.PrintHeader("Chapter 10-E01: Eff 기본");

        // ============================================================
        // 1. Eff vs IO 개념 비교
        // ============================================================
        MenuHelper.PrintSubHeader("1. Eff vs IO 개념 비교");

        MenuHelper.PrintExplanation("IO<A>: 부수 효과를 캡슐화, 성공만 표현");
        MenuHelper.PrintExplanation("Eff<A>: 부수 효과 + 실패(Error)를 함께 표현");
        MenuHelper.PrintExplanation("Eff는 IO + Either의 결합으로 볼 수 있습니다.");
        MenuHelper.PrintBlankLines();

        MenuHelper.PrintCode("// IO<A>는 Fin<A>를 반환 (실행 시 예외가 Fin.Fail로 변환)");
        MenuHelper.PrintCode("// Eff<A>는 설계 시점부터 Error를 고려");
        MenuHelper.PrintBlankLines();

        // IO 예시
        var ioValue = IO.pure(42);
        var ioResult = ioValue.Run();
        MenuHelper.PrintResult("IO.pure(42).Run()", ioResult);

        // Eff 예시
        var effValue = SuccessEff(42);
        var effResult = effValue.Run();
        MenuHelper.PrintResult("SuccessEff(42).Run()", effResult);

        // ============================================================
        // 2. Eff<A> 생성
        // ============================================================
        MenuHelper.PrintSubHeader("2. Eff<A> 생성");

        MenuHelper.PrintExplanation("SuccessEff: 성공 값으로 Eff 생성");
        MenuHelper.PrintExplanation("FailEff: 실패(Error)로 Eff 생성");
        MenuHelper.PrintExplanation("liftEff: 함수를 Eff로 리프팅");
        MenuHelper.PrintBlankLines();

        // 성공 생성
        MenuHelper.PrintCode("var success = SuccessEff(\"Hello, Eff!\");");
        var success = SuccessEff("Hello, Eff!");
        MenuHelper.PrintResult("success.Run()", success.Run());

        // 실패 생성
        MenuHelper.PrintCode("var failure = FailEff<string>(Error.New(\"오류 발생\"));");
        var failure = FailEff<string>(Error.New("오류 발생"));
        MenuHelper.PrintResult("failure.Run()", failure.Run());

        // 함수 리프팅
        MenuHelper.PrintCode("var lifted = liftEff(() => DateTime.Now);");
        var lifted = liftEff(() => DateTime.Now);
        MenuHelper.PrintResult("lifted.Run()", lifted.Run());

        // 예외가 발생할 수 있는 함수
        MenuHelper.PrintCode("var risky = liftEff(() => int.Parse(\"not-a-number\"));");
        var risky = liftEff(() => int.Parse("not-a-number"));
        MenuHelper.PrintResult("risky.Run() [예외 발생]", risky.Run());

        // ============================================================
        // 3. Eff 실행
        // ============================================================
        MenuHelper.PrintSubHeader("3. Eff 실행");

        MenuHelper.PrintExplanation("Run(): Eff 실행, Fin<A> 반환 (성공/실패)");
        MenuHelper.PrintExplanation("RunIO(): IO<A>로 변환 (후속 IO 파이프라인에 사용)");
        MenuHelper.PrintBlankLines();

        var effToRun = SuccessEff(100);

        // Run() 실행
        MenuHelper.PrintCode("var fin = effToRun.Run();");
        var fin = effToRun.Run();
        MenuHelper.PrintResult("fin", fin);
        MenuHelper.PrintResult("fin.IsSucc", fin.IsSucc);

        // Match로 결과 처리
        MenuHelper.PrintCode("fin.Match(Succ: v => ..., Fail: e => ...)");
        var matched = fin.Match(
            Succ: v => $"성공: {v}",
            Fail: e => $"실패: {e.Message}"
        );
        MenuHelper.PrintResult("matched", matched);

        // 실패 Eff 실행
        var failEff = FailEff<int>(Error.New("계산 실패"));
        var failFin = failEff.Run();
        MenuHelper.PrintResult("실패 Eff 결과", failFin);
        MenuHelper.PrintResult("failFin.IsFail", failFin.IsFail);

        // ============================================================
        // 4. Map과 Bind
        // ============================================================
        MenuHelper.PrintSubHeader("4. Map과 Bind");

        MenuHelper.PrintExplanation("Map: Eff 안의 값을 변환 (A -> B)");
        MenuHelper.PrintExplanation("Bind: Eff를 반환하는 함수 연결 (A -> Eff<B>)");
        MenuHelper.PrintBlankLines();

        // Map
        MenuHelper.PrintCode("var mapped = SuccessEff(10).Map(x => x * 2);");
        var mapped = SuccessEff(10).Map(x => x * 2);
        MenuHelper.PrintResult("mapped.Run()", mapped.Run());

        // 체이닝
        var chained = SuccessEff(5)
            .Map(x => x * 2)     // 10
            .Map(x => x + 3)     // 13
            .Map(x => $"결과: {x}");
        MenuHelper.PrintResult("체이닝 결과", chained.Run());

        // Bind
        MenuHelper.PrintCode("var bound = SuccessEff(\"42\").Bind(s => ParseIntEff(s));");

        Eff<int> ParseIntEff(string s) =>
            int.TryParse(s, out var n)
                ? SuccessEff(n)
                : FailEff<int>(Error.New($"'{s}'는 숫자가 아닙니다"));

        var bound = SuccessEff("42").Bind(ParseIntEff);
        MenuHelper.PrintResult("bound.Run()", bound.Run());

        var boundFail = SuccessEff("not-number").Bind(ParseIntEff);
        MenuHelper.PrintResult("파싱 실패", boundFail.Run());

        // 실패 전파
        MenuHelper.PrintExplanation("실패한 Eff에 Map/Bind를 해도 실패가 전파됩니다.");
        var propagated = FailEff<int>(Error.New("초기 실패"))
            .Map(x => x * 2)
            .Map(x => x + 1);
        MenuHelper.PrintResult("실패 전파", propagated.Run());

        // ============================================================
        // 5. LINQ 구문
        // ============================================================
        MenuHelper.PrintSubHeader("5. LINQ 구문");

        MenuHelper.PrintExplanation("from-select 구문으로 Eff를 조합합니다.");
        MenuHelper.PrintBlankLines();

        MenuHelper.PrintCode(@"var linqExample =
    from x in SuccessEff(10)
    from y in SuccessEff(20)
    select x + y;");

        var linqExample =
            from x in SuccessEff(10)
            from y in SuccessEff(20)
            select x + y;

        MenuHelper.PrintResult("linqExample.Run()", linqExample.Run());

        // let 절 사용
        var withLet =
            from x in SuccessEff(42)
            let doubled = x * 2
            let message = $"원본: {x}, 두배: {doubled}"
            select message;

        MenuHelper.PrintResult("let 절 결과", withLet.Run());

        // 중간에 실패가 있으면 전체 실패
        var withFailure =
            from a in SuccessEff(10)
            from b in FailEff<int>(Error.New("중간 실패"))
            from c in SuccessEff(30)  // 실행되지 않음
            select a + b + c;

        MenuHelper.PrintResult("중간 실패", withFailure.Run());

        // ============================================================
        // 6. 에러 처리 기초
        // ============================================================
        MenuHelper.PrintSubHeader("6. 에러 처리 기초");

        MenuHelper.PrintExplanation("Try(): 성공/실패를 Fin으로 감싸서 계속 진행");
        MenuHelper.PrintExplanation("IfFail: 실패 시 대체 값 제공");
        MenuHelper.PrintExplanation("| @catch: 에러 핸들러 파이프라인");
        MenuHelper.PrintBlankLines();

        // IfFail
        MenuHelper.PrintCode("var withDefault = FailEff<int>(...).IfFail(e => 0);");
        var failingEff = FailEff<int>(Error.New("실패!"));
        var withDefault = failingEff.IfFail(_ => 0);
        MenuHelper.PrintResult("IfFail 기본값", withDefault.Run());

        // IfFail with handler
        var withHandler = failingEff.IfFail(err =>
        {
            Console.WriteLine($"    에러 핸들링: {err.Message}");
            return -1;
        });
        MenuHelper.PrintResult("IfFail 핸들러", withHandler.Run());

        // | @catch 연산자 (language-ext 스타일)
        MenuHelper.PrintCode(@"var recovered = failingEff | @catch(e => SuccessEff(999));");

        // @catch를 사용한 에러 복구
        var recovered = failingEff | @catch(e => SuccessEff(999));
        MenuHelper.PrintResult("@catch 복구", recovered.Run());

        // 특정 에러만 catch
        var specificError = Error.New(100, "특정 에러");
        var specificFail = FailEff<string>(specificError);

        var selectiveCatch = specificFail
            | @catch(e => e.Code == 100, SuccessEff("코드 100 에러 복구"))
            | @catch(e => SuccessEff("기타 에러 복구"));

        MenuHelper.PrintResult("선택적 catch", selectiveCatch.Run());

        // ============================================================
        // 7. guard 패턴
        // ============================================================
        MenuHelper.PrintSubHeader("7. guard 패턴");

        MenuHelper.PrintExplanation("guard(조건, 에러): 조건이 false면 실패");
        MenuHelper.PrintExplanation("입력 검증, 비즈니스 규칙 검사에 활용");
        MenuHelper.PrintBlankLines();

        // ---------------------------------------------------------
        // guard와 LINQ 사용 시 주의사항:
        //
        // guard(bool, Error)는 Guard<Error, Unit>을 반환합니다.
        // 이것은 Eff<A>가 아니므로 LINQ 첫 번째 from에 오면 타입 불일치 오류 발생:
        //
        //   ❌ 컴파일 에러:
        //   from _ in guard(cond, error)  // Guard<Error, Unit> - Eff가 아님!
        //   select value;
        //
        // 해결책: Eff가 첫 번째 from에 와야 합니다.
        // Eff<A>.SelectMany(Func<A, Guard<Error, Unit>>, ...) 오버로드가 정의되어 있어서
        // Eff 다음에 오는 guard는 LINQ에서 정상 동작합니다.
        //
        //   ✅ 정상 동작:
        //   from x in SuccessEff(value)   // Eff가 먼저!
        //   from _ in guard(cond, error)  // Eff.SelectMany(Guard) 사용
        //   select x;
        // ---------------------------------------------------------

        Eff<int> ValidatePositive(int value) =>
            value > 0
                ? SuccessEff(value)
                : FailEff<int>(Error.New("양수만 허용됩니다"));

        MenuHelper.PrintCode("guard(value > 0, Error.New(\"양수만 허용됩니다\"))");

        var valid = ValidatePositive(10);
        MenuHelper.PrintResult("ValidatePositive(10)", valid.Run());

        var invalid = ValidatePositive(-5);
        MenuHelper.PrintResult("ValidatePositive(-5)", invalid.Run());

        // ---------------------------------------------------------
        // LINQ에서 guard 사용 패턴:
        // 첫 번째 from에 SuccessEff(value)를 배치하여 Eff 컨텍스트를 먼저 설정합니다.
        // 이후 guard들은 Eff.SelectMany(Guard) 오버로드를 통해 체이닝됩니다.
        // ---------------------------------------------------------
        Eff<string> ValidateUsername(string username) =>
            from u in SuccessEff(username)  // Eff가 먼저 → 이후 guard 사용 가능
            from _1 in guard(notEmpty(u), Error.New("사용자명은 필수입니다"))
            from _2 in guard(u.Length >= 3, Error.New("3자 이상이어야 합니다"))
            from _3 in guard(u.Length <= 20, Error.New("20자 이하여야 합니다"))
            from _4 in guard(u.All(char.IsLetterOrDigit), Error.New("영문자와 숫자만 허용"))
            select u;

        Console.WriteLine();
        MenuHelper.PrintCode(@"// LINQ에서 guard 사용: Eff가 첫 번째 from에 와야 함
Eff<string> ValidateUsername(string username) =>
    from u in SuccessEff(username)  // Eff가 먼저!
    from _1 in guard(notEmpty(u), Error.New(""필수""))
    from _2 in guard(u.Length >= 3, Error.New(""3자 이상""))
    select u;");

        var users = new[] { "john", "ab", "valid_user123", "this_is_way_too_long_username", "bad@name" };
        foreach (var user in users)
        {
            var result = ValidateUsername(user).Run();
            Console.WriteLine($"    \"{user}\": {result}");
        }

        // ============================================================
        // 8. 실전 예제: 간단한 파이프라인
        // ============================================================
        MenuHelper.PrintSubHeader("8. 실전 예제: 간단한 파이프라인");

        MenuHelper.PrintExplanation("사용자 입력을 검증하고 처리하는 파이프라인");
        MenuHelper.PrintBlankLines();

        // 시뮬레이션된 입력
        var inputs = new[] { "25", "-5", "abc", "150", "42" };

        // ---------------------------------------------------------
        // 실전 패턴: Eff를 반환하는 헬퍼 함수 + guard 조합
        //
        // ParseInt처럼 Eff를 반환하는 함수가 있으면,
        // 그 함수 호출이 자연스럽게 첫 번째 from이 되어
        // 이후 guard들과 LINQ로 조합할 수 있습니다.
        //
        // 이 패턴의 장점:
        // 1. 파싱/변환 로직과 검증 로직을 분리
        // 2. 각 단계의 에러가 명확하게 구분됨
        // 3. 선언적이고 읽기 쉬운 코드
        // ---------------------------------------------------------
        Eff<int> ParseInt(string s) =>
            int.TryParse(s, out var n)
                ? SuccessEff(n)
                : FailEff<int>(Error.New($"'{s}'는 숫자가 아닙니다"));

        // ParseInt가 Eff<int>를 반환하므로 첫 번째 from에 자연스럽게 위치
        Eff<int> ProcessAge(string input) =>
            from n in ParseInt(input)  // Eff<int> 반환 → 이후 guard 사용 가능
            from _1 in guard(n >= 0, Error.New("나이는 음수일 수 없습니다"))
            from _2 in guard(n <= 120, Error.New("유효하지 않은 나이입니다"))
            select n;

        MenuHelper.PrintCode(@"// ParseInt가 Eff를 반환하므로 guard와 LINQ 조합 가능
Eff<int> ProcessAge(string input) =>
    from n in ParseInt(input)  // Eff가 먼저!
    from _1 in guard(n >= 0, Error.New(...))
    from _2 in guard(n <= 120, Error.New(...))
    select n;");

        Console.WriteLine();
        foreach (var input in inputs)
        {
            var result = ProcessAge(input).Run();
            var display = result.Match(
                Succ: v => $"유효한 나이: {v}",
                Fail: e => $"오류: {e.Message}"
            );
            Console.WriteLine($"    입력 \"{input}\": {display}");
        }

        // 전체 파이프라인 with 에러 복구
        MenuHelper.PrintBlankLines();
        MenuHelper.PrintCode("// 에러 복구를 포함한 파이프라인");

        var pipeline =
            from age in ProcessAge("abc")
                | @catch(e => SuccessEff(0))  // 파싱 실패 시 0
            from category in age switch
            {
                < 18 => SuccessEff("미성년자"),
                < 65 => SuccessEff("성인"),
                _ => SuccessEff("시니어")
            }
            select $"나이: {age}, 카테고리: {category}";

        MenuHelper.PrintResult("복구 파이프라인", pipeline.Run());

        MenuHelper.PrintSuccess("Eff 기본 학습 완료!");
    }
}
