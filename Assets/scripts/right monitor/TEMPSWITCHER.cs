using UnityEngine;

public class SiteManager : MonoBehaviour
{
    public GameObject S1;
    public GameObject S2;
    public GameObject S3;

    public void ShowS1()
    {
        S1.SetActive(true);
        S2.SetActive(false);
        S3.SetActive(false);
        Debug.Log("✅ Showing Site 1");
    }

    public void ShowS2()
    {
        S1.SetActive(false);
        S2.SetActive(true);
        S3.SetActive(false);
        Debug.Log("✅ Showing Site 2");
    }

    public void ShowS3()
    {
        S1.SetActive(false);
        S2.SetActive(false);
        S3.SetActive(true);
        Debug.Log("✅ Showing Site 3");
    }
}
