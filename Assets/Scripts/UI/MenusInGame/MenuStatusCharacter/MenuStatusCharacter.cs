using System.Collections;
using System.Threading.Tasks;
using UnityEngine;

public class MenuStatusCharacter : MonoBehaviour
{
    public PlayerManager playerManager;
    public GameObject menuStatusCharacter;
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }
    public IEnumerator EnableMenu()
    {
        yield return SetCharacterData();
        menuStatusCharacter.SetActive(true);
        playerManager.menuCharacterActions.DisableMenu(true);
        playerManager.menuCharacterInfo.menuCharacterInfo.SetActive(false);
        yield return null;
    }
    public async Task SetCharacterData()
    {
        await Awaitable.NextFrameAsync();
    }
    public void DisableMenu()
    {
        playerManager.menuCharacterActions.BackToMenuWhitButton(MenuCharacterActions.TypeButton.Item);
    }
}
