using UnityEngine;

namespace MyFirstAnimationExample
{
    public class MyFirstAnimationScript : MonoBehaviour
    {
        private Animator _animator;
        void Start()
        {
            _animator = GetComponentInChildren<Animator>(true);
            _animator.speed = 1.0f;
        }
    }
}
