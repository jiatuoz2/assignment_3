using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro; 

public class NotifierSystem : MonoBehaviour
{
    public static NotifierSystem Instance; 
    public GameObject notifier; 
    private Coroutine currentCoroutine = null; 

    // Start is called before the first frame update
    void Start()
    {
        if (Instance == null) {
            Instance = this;
        } else {
            Destroy(gameObject); 
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void ShowNotifier(string description, string title, int seconds) {
        if (currentCoroutine != null) {
            StopCoroutine(currentCoroutine);
        }
        currentCoroutine = StartCoroutine(DisplayNotifier(description, title, seconds));
    }

    public IEnumerator DisplayNotifier(string description, string title, int seconds) {
        TextMeshProUGUI _title = notifier.transform.Find("Title Text").GetComponent<TextMeshProUGUI>();
        TextMeshProUGUI _description = notifier.transform.Find("Description Text").GetComponent<TextMeshProUGUI>();
        _title.text = title;
        _description.text = description;
        notifier.SetActive(true); 
        yield return new WaitForSeconds(seconds);
        notifier.SetActive(false);
        currentCoroutine = null;
    }
}
