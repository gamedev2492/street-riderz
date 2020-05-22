using UnityEngine;

namespace Supragma
{
    public class UserInput : MonoBehaviour
    {
        float horizontal;
        float vertical;

        // Update is called once per frame
        void Update()
        {
            horizontal = Input.GetAxis("Horizontal");
            vertical = Input.GetAxis("Vertical");
        }
    }
}
