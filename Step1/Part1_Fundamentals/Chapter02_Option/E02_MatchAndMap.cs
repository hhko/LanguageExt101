using LanguageExt;
using LanguageExt101.Common;
using static LanguageExt.Prelude;

namespace LanguageExt101.Part1_Fundamentals.Chapter02_Option;

/// <summary>
/// Option의 Match와 Map 패턴을 학습합니다.
///
/// 학습 목표:
/// - Match를 사용한 패턴 매칭
/// - Map을 사용한 값 변환
/// - IfNone, IfSome 헬퍼 메서드
/// - Match vs if문의 차이
///
/// 핵심 개념:
/// Match는 Option의 두 가지 상태(Some/None)를 처리하는 함수형 패턴입니다.
/// Map은 Option 내부의 값을 변환하며, None인 경우 자동으로 None을 유지합니다.
/// 이 패턴들을 사용하면 null 체크 없이 안전하게 값을 처리할 수 있습니다.
/// </summary>
public static class E02_MatchAndMap
{
    /// <summary>
    /// 예제를 실행합니다.
    /// </summary>
    public static void Run()
    {
        MenuHelper.PrintHeader("Chapter 02-E02: Match와 Map");

        // ============================================================
        // 1. Match 기본 사용법
        // ============================================================
        MenuHelper.PrintSubHeader("1. Match 기본 사용법");

        MenuHelper.PrintExplanation("Match는 Option의 두 상태를 모두 처리해야 합니다.");
        MenuHelper.PrintExplanation("컴파일러가 두 케이스 모두 처리했는지 확인합니다.");
        MenuHelper.PrintBlankLines();

        var someNumber = Some(42);
        Option<int> noNumber = None;

        MenuHelper.PrintCode("// Some 케이스와 None 케이스 모두 처리");
        MenuHelper.PrintCode("someNumber.Match(Some: x => $\"값: {x}\", None: () => \"값 없음\")");

        var result1 = someNumber.Match(
            Some: x => $"값: {x}",
            None: () => "값 없음"
        );
        MenuHelper.PrintResult("Some(42) Match 결과", result1);

        var result2 = noNumber.Match(
            Some: x => $"값: {x}",
            None: () => "값 없음"
        );
        MenuHelper.PrintResult("None Match 결과", result2);

        // ============================================================
        // 2. Match로 다른 타입 반환
        // ============================================================
        MenuHelper.PrintSubHeader("2. Match로 다른 타입 반환");

        MenuHelper.PrintExplanation("Match는 Option<A>를 완전히 다른 타입 B로 변환할 수 있습니다.");
        MenuHelper.PrintBlankLines();

        var user = Some(new User(1, "Alice", 30));

        // Option<User> → string 변환
        var greeting = user.Match(
            Some: u => $"안녕하세요, {u.Name}님! (나이: {u.Age}세)",
            None: () => "손님, 로그인해주세요."
        );
        MenuHelper.PrintResult("사용자 인사말", greeting);

        // Option<User> → int 변환
        var ageOrDefault = user.Match(
            Some: u => u.Age,
            None: () => 0
        );
        MenuHelper.PrintResult("사용자 나이 (기본값 0)", ageOrDefault);

        // ============================================================
        // 3. Map 기본 사용법
        // ============================================================
        MenuHelper.PrintSubHeader("3. Map 기본 사용법");

        MenuHelper.PrintExplanation("Map은 Option 내부의 값만 변환합니다.");
        MenuHelper.PrintExplanation("None인 경우 자동으로 None이 유지됩니다.");
        MenuHelper.PrintExplanation("Option<A> → Option<B> 변환에 사용합니다.");
        MenuHelper.PrintBlankLines();

        var number = Some(10);
        Option<int> noNum = None;

        MenuHelper.PrintCode("var doubled = number.Map(x => x * 2);");
        var doubled = number.Map(x => x * 2);
        MenuHelper.PrintResult("Some(10).Map(x => x * 2)", doubled);

        var doubledNone = noNum.Map(x => x * 2);
        MenuHelper.PrintResult("None.Map(x => x * 2)", doubledNone);
        MenuHelper.PrintBlankLines();

        // 연속 Map (체이닝)
        MenuHelper.PrintCode("// Map 체이닝");
        var processed = Some(5)
            .Map(x => x * 2)      // 10
            .Map(x => x + 3)      // 13
            .Map(x => $"결과: {x}"); // "결과: 13"

        MenuHelper.PrintResult("Some(5).Map(*2).Map(+3).Map(문자열)", processed);

        // ============================================================
        // 4. Select (LINQ 스타일 Map)
        // ============================================================
        MenuHelper.PrintSubHeader("4. Select (LINQ 스타일)");

        MenuHelper.PrintExplanation("Select는 Map과 동일하며 LINQ와의 호환성을 제공합니다.");
        MenuHelper.PrintBlankLines();

        var selected = Some(100).Select(x => x / 2);
        MenuHelper.PrintResult("Some(100).Select(x => x / 2)", selected);

        // ============================================================
        // 5. IfSome과 IfNone
        // ============================================================
        MenuHelper.PrintSubHeader("5. IfSome과 IfNone");

        MenuHelper.PrintExplanation("한쪽 케이스만 처리하고 싶을 때 사용합니다.");
        MenuHelper.PrintExplanation("주로 부수 효과(로깅, 알림 등)에 사용됩니다.");
        MenuHelper.PrintBlankLines();

        var maybeValue = Some("존재하는 값");
        Option<string> empty = None;

        MenuHelper.PrintCode("// IfSome: Some일 때만 실행");
        maybeValue.IfSome(v => Console.WriteLine($"  IfSome 실행: {v}"));
        empty.IfSome(v => Console.WriteLine($"  IfSome 실행: {v}"));
        MenuHelper.PrintExplanation("empty.IfSome은 실행되지 않음");
        MenuHelper.PrintBlankLines();

        MenuHelper.PrintCode("// IfNone: None일 때만 실행");
        maybeValue.IfNone(() => Console.WriteLine("  IfNone 실행: 값 없음"));
        empty.IfNone(() => Console.WriteLine("  IfNone 실행: 값 없음"));

        // ============================================================
        // 6. IfNone으로 기본값 제공
        // ============================================================
        MenuHelper.PrintSubHeader("6. IfNone으로 기본값 제공");

        MenuHelper.PrintExplanation("IfNone(defaultValue)는 None일 때 기본값을 반환합니다.");
        MenuHelper.PrintExplanation("Match의 간단한 버전이라고 생각하면 됩니다.");
        MenuHelper.PrintBlankLines();

        var withValue = Some(42);
        Option<int> withoutValue = None;

        MenuHelper.PrintCode("var result = option.IfNone(0);");
        var r1 = withValue.IfNone(0);
        var r2 = withoutValue.IfNone(0);

        MenuHelper.PrintResult("Some(42).IfNone(0)", r1);
        MenuHelper.PrintResult("None.IfNone(0)", r2);
        MenuHelper.PrintBlankLines();

        // 지연 평가 버전
        MenuHelper.PrintCode("// 지연 평가: IfNone(() => ExpensiveCalculation())");
        var lazy = withoutValue.IfNone(() =>
        {
            Console.WriteLine("  지연 평가 실행됨!");
            return 999;
        });
        MenuHelper.PrintResult("지연 평가 결과", lazy);

        // ============================================================
        // 7. Match vs if문 비교
        // ============================================================
        MenuHelper.PrintSubHeader("7. Match vs if문 비교");

        MenuHelper.PrintExplanation("Match는 컴파일 타임에 모든 케이스 처리를 보장합니다.");
        MenuHelper.PrintExplanation("if문은 케이스 누락 가능성이 있습니다.");
        MenuHelper.PrintBlankLines();

        var optUser = Some(new User(2, "Bob", 25));

        // if문 스타일 (권장하지 않음)
        MenuHelper.PrintCode("// if문 스타일 (권장하지 않음)");
        MenuHelper.PrintCode("if (optUser.IsSome) { /* ... */ }");
        MenuHelper.PrintBlankLines();

        // Match 스타일 (권장)
        MenuHelper.PrintCode("// Match 스타일 (권장)");
        MenuHelper.PrintCode("optUser.Match(Some: ..., None: ...)");

        var matchResult = optUser.Match(
            Some: u => $"사용자: {u.Name}",
            None: () => "사용자 없음"
        );
        MenuHelper.PrintResult("Match 결과", matchResult);

        // ============================================================
        // 8. 실전 예제: 설정 값 처리
        // ============================================================
        MenuHelper.PrintSubHeader("8. 실전 예제: 설정 값 처리");

        var settings = new Dictionary<string, string>
        {
            { "theme", "dark" },
            { "language", "ko" }
        };

        var theme = GetSetting(settings, "theme")
            .Map(t => t.ToUpper())
            .IfNone("LIGHT");

        var fontSize = GetSetting(settings, "fontSize")
            .Map(int.Parse)
            .IfNone(14);

        MenuHelper.PrintResult("테마 설정", theme);
        MenuHelper.PrintResult("폰트 크기 (기본값 14)", fontSize);

        MenuHelper.PrintSuccess("Match와 Map 학습 완료!");
    }

    private static Option<string> GetSetting(Dictionary<string, string> settings, string key)
    {
        return settings.TryGetValue(key, out var value) ? Some(value) : None;
    }

    private record User(int Id, string Name, int Age);
}
