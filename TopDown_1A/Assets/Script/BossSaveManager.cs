using System.IO;
using UnityEngine;

public static class BossSaveManager
{
    // 📂 저장될 파일의 로컬 경로를 가져옵니다.
    private static string GetSavePath()
    {
        // Application.persistentDataPath는 게임이 꺼져도 날아가지 않는 안전한 운영체제별 기본 저장 경로입니다.
        return Path.Combine(Application.persistentDataPath, "CapturedBossSoul.json");
    }

    // 💾 보스 영혼 데이터를 제이슨으로 저장합니다.
    public static void SaveBossSoul(BossSoulData data)
    {
        // 객체를 JSON 문자열로 변환 (true를 넣으면 보기 좋게 줄바꿈이 됩니다)
        string json = JsonUtility.ToJson(data, true); 
        
        // 실제 파일로 쓰기
        File.WriteAllText(GetSavePath(), json);
        
        Debug.Log($"💾 [Json 저장 성공] 경로: {GetSavePath()}\n내용:\n{json}");
    }

    // 📂 저장된 제이슨 파일을 읽어옵니다.
    public static BossSoulData LoadBossSoul()
    {
        string path = GetSavePath();
        
        // 파일이 존재하는지 먼저 확인
        if (File.Exists(path))
        {
            string json = File.ReadAllText(path);
            BossSoulData data = JsonUtility.FromJson<BossSoulData>(json);
            return data;
        }
        
        // 파일이 없다면 null을 반환
        Debug.LogWarning("📂 [Json 로드 실패] 저장된 보스 영혼 데이터 파일이 없습니다.");
        return null;
    }
}