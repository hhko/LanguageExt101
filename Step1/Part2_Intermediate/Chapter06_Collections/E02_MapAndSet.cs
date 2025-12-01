using LanguageExt;
using LanguageExt101.Common;
using static LanguageExt.Prelude;

namespace LanguageExt101.Part2_Intermediate.Chapter06_Collections;

/// <summary>
/// Map과 Set 컬렉션을 학습합니다.
///
/// 학습 목표:
/// - Map<K, V>: 불변 딕셔너리
/// - Set<T>: 불변 집합
/// - HashMap<K, V>: 해시 기반 불변 딕셔너리
/// - 집합 연산 (합집합, 교집합, 차집합)
///
/// 핵심 개념:
/// LanguageExt의 컬렉션은 모두 불변입니다.
/// 수정 연산은 새 컬렉션을 반환하며, 구조적 공유로 효율성을 유지합니다.
/// </summary>
public static class E02_MapAndSet
{
    /// <summary>
    /// 예제를 실행합니다.
    /// </summary>
    public static void Run()
    {
        MenuHelper.PrintHeader("Chapter 06-E02: Map과 Set");

        // ============================================================
        // 1. Map 생성
        // ============================================================
        MenuHelper.PrintSubHeader("1. Map 생성");

        MenuHelper.PrintExplanation("Map<K, V>는 불변 키-값 딕셔너리입니다.");
        MenuHelper.PrintBlankLines();

        // 튜플로 생성
        MenuHelper.PrintCode("var map1 = Map((\"a\", 1), (\"b\", 2), (\"c\", 3));");
        var map1 = Map(("a", 1), ("b", 2), ("c", 3));
        MenuHelper.PrintResult("Map", map1);

        // 빈 Map
        var emptyMap = Map<string, int>();
        MenuHelper.PrintResult("Empty Map", emptyMap);

        // ============================================================
        // 2. Map 조회
        // ============================================================
        MenuHelper.PrintSubHeader("2. Map 조회");

        MenuHelper.PrintExplanation("Find는 Option<V>를 반환합니다.");
        MenuHelper.PrintBlankLines();

        var found = map1.Find("a");
        var notFound = map1.Find("z");

        MenuHelper.PrintResult("Find(\"a\")", found);
        MenuHelper.PrintResult("Find(\"z\")", notFound);

        // 인덱서 (Option 반환)
        var indexed = map1["b"];
        MenuHelper.PrintResult("map1[\"b\"]", indexed);

        // ContainsKey
        MenuHelper.PrintResult("ContainsKey(\"a\")", map1.ContainsKey("a"));
        MenuHelper.PrintResult("ContainsKey(\"z\")", map1.ContainsKey("z"));

        // ============================================================
        // 3. Map 수정
        // ============================================================
        MenuHelper.PrintSubHeader("3. Map 수정");

        MenuHelper.PrintExplanation("수정 연산은 새 Map을 반환합니다 (불변성).");
        MenuHelper.PrintBlankLines();

        // 추가
        var withD = map1.Add("d", 4);
        MenuHelper.PrintResult("Add(\"d\", 4)", withD);
        MenuHelper.PrintResult("원본 (변경 없음)", map1);

        // 업데이트 (있으면 수정, 없으면 추가)
        var updated = map1.AddOrUpdate("a", 100);
        MenuHelper.PrintResult("AddOrUpdate(\"a\", 100)", updated);

        // 제거
        var removed = map1.Remove("b");
        MenuHelper.PrintResult("Remove(\"b\")", removed);

        // ============================================================
        // 4. Map 변환
        // ============================================================
        MenuHelper.PrintSubHeader("4. Map 변환");

        var scores = Map(("Alice", 85), ("Bob", 92), ("Charlie", 78));

        // Map (값 변환)
        var curved = scores.Map(v => v + 5);
        MenuHelper.PrintResult("점수 +5", curved);

        // Filter
        var passed = scores.Filter(v => v >= 80);
        MenuHelper.PrintResult("80점 이상", passed);

        // Keys와 Values
        MenuHelper.PrintResult("Keys", scores.Keys);
        MenuHelper.PrintResult("Values", scores.Values);

        // ============================================================
        // 5. Set 생성
        // ============================================================
        MenuHelper.PrintSubHeader("5. Set 생성");

        MenuHelper.PrintExplanation("Set<T>는 불변 집합으로, 중복을 허용하지 않습니다.");
        MenuHelper.PrintBlankLines();

        var set1 = Set(1, 2, 3, 4, 5);
        var set2 = Set(3, 4, 5, 6, 7);
        var setWithDupes = Set(1, 1, 2, 2, 3);

        MenuHelper.PrintResult("Set(1,2,3,4,5)", set1);
        MenuHelper.PrintResult("Set(3,4,5,6,7)", set2);
        MenuHelper.PrintResult("Set(1,1,2,2,3) - 중복 제거", setWithDupes);

        // ============================================================
        // 6. Set 연산
        // ============================================================
        MenuHelper.PrintSubHeader("6. Set 연산");

        // 합집합
        var union = set1.Union(set2);
        MenuHelper.PrintResult("합집합 (Union)", union);

        // 교집합
        var intersection = set1.Intersect(set2);
        MenuHelper.PrintResult("교집합 (Intersect)", intersection);

        // 차집합
        var except = set1.Except(set2);
        MenuHelper.PrintResult("차집합 (Except)", except);

        // 대칭 차집합
        var symmetric = set1.SymmetricExcept(set2);
        MenuHelper.PrintResult("대칭 차집합", symmetric);

        // ============================================================
        // 7. Set 조회 및 수정
        // ============================================================
        MenuHelper.PrintSubHeader("7. Set 조회 및 수정");

        var numbers = Set(10, 20, 30);

        MenuHelper.PrintResult("Contains(20)", numbers.Contains(20));
        MenuHelper.PrintResult("Contains(99)", numbers.Contains(99));

        var added = numbers.Add(40);
        MenuHelper.PrintResult("Add(40)", added);

        var removedSet = numbers.Remove(20);
        MenuHelper.PrintResult("Remove(20)", removedSet);

        // ============================================================
        // 8. HashMap (해시 기반)
        // ============================================================
        MenuHelper.PrintSubHeader("8. HashMap (해시 기반)");

        MenuHelper.PrintExplanation("HashMap은 해시 테이블 기반으로 빠른 조회를 제공합니다.");
        MenuHelper.PrintExplanation("Map은 트리 기반으로 정렬된 순서를 유지합니다.");
        MenuHelper.PrintBlankLines();

        var hashMap = HashMap(("key1", "value1"), ("key2", "value2"));
        MenuHelper.PrintResult("HashMap", hashMap);

        var foundInHash = hashMap.Find("key1");
        MenuHelper.PrintResult("Find(\"key1\")", foundInHash);

        // ============================================================
        // 9. 실전 예제: 사용자 그룹 관리
        // ============================================================
        MenuHelper.PrintSubHeader("9. 실전 예제: 사용자 그룹 관리");

        var admins = Set("alice", "bob");
        var editors = Set("bob", "charlie");
        var viewers = Set("charlie", "diana", "eve");

        // 모든 사용자
        var allUsers = admins.Union(editors).Union(viewers);
        MenuHelper.PrintResult("모든 사용자", allUsers);

        // 여러 권한을 가진 사용자
        var multiRole = admins.Intersect(editors);
        MenuHelper.PrintResult("관리자이면서 편집자", multiRole);

        // 뷰어만 있는 사용자
        var viewersOnly = viewers.Except(admins).Except(editors);
        MenuHelper.PrintResult("뷰어 권한만 있는 사용자", viewersOnly);

        // 권한 Map
        var permissions = Map(
            ("alice", Set("read", "write", "delete")),
            ("bob", Set("read", "write")),
            ("charlie", Set("read"))
        );

        var alicePerms = permissions.Find("alice").IfNone(Set<string>());
        MenuHelper.PrintResult("Alice 권한", alicePerms);

        MenuHelper.PrintSuccess("Map과 Set 학습 완료!");
    }
}
