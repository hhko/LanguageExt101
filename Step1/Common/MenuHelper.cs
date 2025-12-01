namespace LanguageExt101.Common;

/// <summary>
/// 콘솔 메뉴 출력을 위한 헬퍼 클래스입니다.
///
/// 이 클래스는 일관된 메뉴 UI를 제공하고,
/// 사용자 입력을 처리하는 공통 기능을 담당합니다.
/// </summary>
public static class MenuHelper
{
    /// <summary>
    /// 메뉴 항목을 표시하고 사용자 선택을 받습니다.
    /// </summary>
    /// <param name="title">메뉴 제목</param>
    /// <param name="options">선택 가능한 옵션 목록 (번호, 설명)</param>
    /// <returns>사용자가 선택한 번호 (0은 뒤로가기/종료)</returns>
    public static int ShowMenu(string title, params (int number, string description)[] options)
    {
        Console.WriteLine();
        Console.WriteLine(new string('=', 60));
        Console.WriteLine($"  {title}");
        Console.WriteLine(new string('=', 60));
        Console.WriteLine();

        foreach (var (number, description) in options)
        {
            Console.WriteLine($"  [{number}] {description}");
        }

        Console.WriteLine();
        Console.WriteLine("  [0] 뒤로가기 / 종료");
        Console.WriteLine();
        Console.Write("  선택: ");

        if (int.TryParse(Console.ReadLine(), out int choice))
        {
            return choice;
        }

        return -1; // 잘못된 입력
    }

    /// <summary>
    /// 섹션 헤더를 출력합니다.
    /// </summary>
    /// <param name="title">섹션 제목</param>
    public static void PrintHeader(string title)
    {
        Console.WriteLine();
        Console.WriteLine(new string('=', 60));
        Console.WriteLine($"  {title}");
        Console.WriteLine(new string('=', 60));
        Console.WriteLine();
    }

    /// <summary>
    /// 서브섹션 헤더를 출력합니다.
    /// </summary>
    /// <param name="title">서브섹션 제목</param>
    public static void PrintSubHeader(string title)
    {
        Console.WriteLine();
        Console.WriteLine($"--- {title} ---");
        Console.WriteLine();
    }

    /// <summary>
    /// 예제 결과를 출력합니다.
    /// </summary>
    /// <param name="label">레이블</param>
    /// <param name="value">값</param>
    public static void PrintResult(string label, object? value)
    {
        Console.WriteLine($"  {label}: {value}");
    }

    /// <summary>
    /// 설명 텍스트를 출력합니다.
    /// </summary>
    /// <param name="text">설명 텍스트</param>
    public static void PrintExplanation(string text)
    {
        Console.ForegroundColor = ConsoleColor.DarkGray;
        Console.WriteLine($"  // {text}");
        Console.ResetColor();
    }

    /// <summary>
    /// 코드 예제를 출력합니다.
    /// </summary>
    /// <param name="code">코드 문자열</param>
    public static void PrintCode(string code)
    {
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine($"  {code}");
        Console.ResetColor();
    }

    /// <summary>
    /// 경고 메시지를 출력합니다.
    /// </summary>
    /// <param name="message">경고 메시지</param>
    public static void PrintWarning(string message)
    {
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine($"  ⚠ {message}");
        Console.ResetColor();
    }

    /// <summary>
    /// 성공 메시지를 출력합니다.
    /// </summary>
    /// <param name="message">성공 메시지</param>
    public static void PrintSuccess(string message)
    {
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine($"  ✓ {message}");
        Console.ResetColor();
    }

    /// <summary>
    /// 에러 메시지를 출력합니다.
    /// </summary>
    /// <param name="message">에러 메시지</param>
    public static void PrintError(string message)
    {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine($"  ✗ {message}");
        Console.ResetColor();
    }

    /// <summary>
    /// 계속하려면 아무 키나 누르세요 메시지를 표시합니다.
    /// </summary>
    public static void WaitForKey()
    {
        Console.WriteLine();
        Console.WriteLine("  계속하려면 아무 키나 누르세요...");
        Console.ReadKey(true);
    }

    /// <summary>
    /// 빈 줄을 출력합니다.
    /// </summary>
    /// <param name="count">빈 줄 수</param>
    public static void PrintBlankLines(int count = 1)
    {
        for (int i = 0; i < count; i++)
        {
            Console.WriteLine();
        }
    }
}
