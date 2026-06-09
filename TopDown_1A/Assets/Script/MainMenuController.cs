using UnityEngine;
using UnityEngine.SceneManagement; // 씬 전환을 위해 반드시 필요한 네임스페이스

public class MainMenuController : MonoBehaviour
{
    [Header("이동할 스테이지 이름")]
    [SerializeField] private string firstStageName = "Level_1"; // 레벨 1 씬 이름을 적어줍니다.

    /// <summary>
    /// 시작 버튼을 눌렀을 때 호출될 함수
    /// </summary>
    public void OnStartButtonClick()
    {
        // [최적화 팁] 씬 전환 시 가비지 컬렉터(GC) 렉을 줄이기 위해 싱글 스레드로 즉시 로드합니다.
        // 만약 로딩 화면이 필요할 정도로 무거운 맵이라면 LoadSceneAsync를 쓰는 것이 좋습니다.
        Debug.Log($"{firstStageName} 스테이지로 이동합니다.");
        
        SceneManager.LoadScene(firstStageName);
    }

    /// <summary>
    /// 게임 종료 버튼을 눌렀을 때 호출될 함수 (옵션)
    /// </summary>
    public void OnExitButtonClick()
    {
        Debug.Log("게임이 종료됩니다.");
        
        #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false; // 에디터에서 실행 중일 때 종료
        #else
            Application.Quit(); // 실제 빌드된 게임에서 종료
        #endif
    }
}