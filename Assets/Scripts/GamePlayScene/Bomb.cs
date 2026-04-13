using UnityEngine;
using System.Threading.Tasks;

public class Bomb : MonoBehaviour
{
    async void Start()
    {
        //AudioManager.Instance.PlaySFX(SFX.Bomb);

        await Task.Delay((int)(2 * 1000));

        if (this == null || gameObject == null)
            return;

        DestroySelf();
    }

    void DestroySelf()
    {
        Destroy(gameObject);
    }
}
