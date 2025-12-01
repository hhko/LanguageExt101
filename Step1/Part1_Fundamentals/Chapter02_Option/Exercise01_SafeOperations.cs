using LanguageExt;
using LanguageExt101.Common;
using static LanguageExt.Prelude;

namespace LanguageExt101.Part1_Fundamentals.Chapter02_Option;

/// <summary>
/// 실습 01: Option을 사용한 안전한 연산
///
/// 학습 목표:
/// - Option을 사용하여 예외 없이 안전한 연산 구현
/// - 체이닝을 통한 복잡한 로직 처리
/// - 실제 시나리오에서 Option 활용
///
/// 과제:
/// 1. SafeDivide: 0으로 나누기를 안전하게 처리
/// 2. SafeParse: 문자열을 숫자로 안전하게 변환
/// 3. GetNestedValue: 중첩된 딕셔너리에서 값 안전하게 가져오기
/// 4. FindFirstPositive: 리스트에서 첫 번째 양수 찾기
/// </summary>
public static class Exercise01_SafeOperations
{
    /// <summary>
    /// 예제를 실행합니다.
    /// </summary>
    public static void Run()
    {
        MenuHelper.PrintHeader("Exercise 01: 안전한 연산");

        // ============================================================
        // 과제 1: SafeDivide
        // ============================================================
        MenuHelper.PrintSubHeader("과제 1: SafeDivide - 안전한 나눗셈");

        MenuHelper.PrintExplanation("0으로 나누면 None을 반환하는 함수를 구현하세요.");
        MenuHelper.PrintExplanation("힌트: divisor가 0인지 확인 후 Some 또는 None 반환");
        MenuHelper.PrintBlankLines();

        // 테스트
        var div1 = SafeDivide(10, 2);
        var div2 = SafeDivide(10, 0);
        var div3 = SafeDivide(15, 3);

        MenuHelper.PrintResult("10 / 2", div1);
        MenuHelper.PrintResult("10 / 0", div2);
        MenuHelper.PrintResult("15 / 3", div3);

        // ============================================================
        // 과제 2: SafeParse
        // ============================================================
        MenuHelper.PrintSubHeader("과제 2: SafeParse - 안전한 파싱");

        MenuHelper.PrintExplanation("문자열을 int로 파싱하고, 실패하면 None을 반환하세요.");
        MenuHelper.PrintExplanation("힌트: int.TryParse 사용");
        MenuHelper.PrintBlankLines();

        var parse1 = SafeParseInt("42");
        var parse2 = SafeParseInt("abc");
        var parse3 = SafeParseInt("  123  ");

        MenuHelper.PrintResult("\"42\" 파싱", parse1);
        MenuHelper.PrintResult("\"abc\" 파싱", parse2);
        MenuHelper.PrintResult("\"  123  \" 파싱", parse3);

        // ============================================================
        // 과제 3: GetNestedValue
        // ============================================================
        MenuHelper.PrintSubHeader("과제 3: GetNestedValue - 중첩 딕셔너리");

        MenuHelper.PrintExplanation("중첩된 딕셔너리에서 값을 안전하게 가져오세요.");
        MenuHelper.PrintExplanation("힌트: Bind를 사용하여 체이닝");
        MenuHelper.PrintBlankLines();

        var config = new Dictionary<string, Dictionary<string, string>>
        {
            { "database", new Dictionary<string, string>
                {
                    { "host", "localhost" },
                    { "port", "5432" }
                }
            },
            { "server", new Dictionary<string, string>
                {
                    { "port", "8080" }
                }
            }
        };

        var dbHost = GetNestedValue(config, "database", "host");
        var dbPassword = GetNestedValue(config, "database", "password");
        var cacheHost = GetNestedValue(config, "cache", "host");

        MenuHelper.PrintResult("database.host", dbHost);
        MenuHelper.PrintResult("database.password", dbPassword);
        MenuHelper.PrintResult("cache.host", cacheHost);

        // ============================================================
        // 과제 4: 체이닝 응용
        // ============================================================
        MenuHelper.PrintSubHeader("과제 4: 체이닝 응용");

        MenuHelper.PrintExplanation("SafeParseInt와 SafeDivide를 체이닝하여");
        MenuHelper.PrintExplanation("문자열 두 개를 받아 나눗셈 결과를 반환하세요.");
        MenuHelper.PrintBlankLines();

        var chain1 = ParseAndDivide("10", "2");
        var chain2 = ParseAndDivide("10", "0");
        var chain3 = ParseAndDivide("abc", "2");

        MenuHelper.PrintResult("\"10\" / \"2\"", chain1);
        MenuHelper.PrintResult("\"10\" / \"0\"", chain2);
        MenuHelper.PrintResult("\"abc\" / \"2\"", chain3);

        // ============================================================
        // 과제 5: 첫 번째 양수 찾기
        // ============================================================
        MenuHelper.PrintSubHeader("과제 5: FindFirstPositive");

        MenuHelper.PrintExplanation("리스트에서 첫 번째 양수를 찾으세요.");
        MenuHelper.PrintExplanation("힌트: List의 Find 메서드 활용");
        MenuHelper.PrintBlankLines();

        var list1 = List(-1, -2, 3, 4, 5);
        var list2 = List(-1, -2, -3);
        var list3 = Lst<int>.Empty;

        MenuHelper.PrintResult("[-1, -2, 3, 4, 5]에서 첫 양수", FindFirstPositive(list1));
        MenuHelper.PrintResult("[-1, -2, -3]에서 첫 양수", FindFirstPositive(list2));
        MenuHelper.PrintResult("[]에서 첫 양수", FindFirstPositive(list3));

        MenuHelper.PrintSuccess("실습 01 완료! Solutions 폴더에서 정답을 확인하세요.");
    }

    // ============================================================
    // 구현할 함수들
    // ============================================================

    /// <summary>
    /// 안전한 나눗셈: 0으로 나누면 None 반환
    /// </summary>
    private static Option<int> SafeDivide(int dividend, int divisor)
    {
        // TODO: 구현하세요
        // 힌트: divisor == 0 이면 None, 아니면 Some(dividend / divisor)
        return divisor == 0
            ? None
            : Some(dividend / divisor);
    }

    /// <summary>
    /// 안전한 int 파싱: 실패하면 None 반환
    /// </summary>
    private static Option<int> SafeParseInt(string input)
    {
        // TODO: 구현하세요
        // 힌트: int.TryParse 사용
        return int.TryParse(input, out var result)
            ? Some(result)
            : None;
    }

    /// <summary>
    /// 중첩 딕셔너리에서 값 가져오기
    /// </summary>
    private static Option<string> GetNestedValue(
        Dictionary<string, Dictionary<string, string>> dict,
        string outerKey,
        string innerKey)
    {
        // TODO: 구현하세요
        // 힌트: TryGetValue를 Option으로 감싸고 Bind 사용
        return dict.TryGetValue(outerKey, out var inner)
            ? inner.TryGetValue(innerKey, out var value)
                ? Some(value)
                : None
            : None;

        // 또는 LINQ 스타일:
        // return Optional(dict.GetValueOrDefault(outerKey))
        //     .Bind(inner => Optional(inner.GetValueOrDefault(innerKey)));
    }

    /// <summary>
    /// 문자열 두 개를 파싱하여 나눗셈
    /// </summary>
    private static Option<int> ParseAndDivide(string numStr, string denomStr)
    {
        // TODO: 구현하세요
        // 힌트: SafeParseInt와 SafeDivide를 Bind로 연결
        return from num in SafeParseInt(numStr)
               from denom in SafeParseInt(denomStr)
               from result in SafeDivide(num, denom)
               select result;

        // 또는 메서드 체이닝:
        // return SafeParseInt(numStr)
        //     .Bind(num => SafeParseInt(denomStr)
        //         .Bind(denom => SafeDivide(num, denom)));
    }

    /// <summary>
    /// 리스트에서 첫 번째 양수 찾기
    /// </summary>
    private static Option<int> FindFirstPositive(Lst<int> numbers)
    {
        // TODO: 구현하세요
        // 힌트: numbers.Find(x => x > 0) 사용
        return numbers.Find(x => x > 0);
    }
}
