using UnityEngine;

namespace Game.UI
{
    public class LuYingCharacterController : MonoBehaviour
    {
        [SerializeField] private float blinkIntervalMin = 2f;
        [SerializeField] private float blinkIntervalMax = 6f;

        private Animator animator;
        private float nextBlinkTime;

        private const string StateIdle = "Idle";
        private const string StateLittleExcitedIdle = "LittleExcitedIdle";
        private const string StateExcitedIdle = "ExcitedIdle";
        private const string StateBlink = "Blink";
        private const string StateChew = "Chew";

        private static readonly string[] MoodIdleStates = { StateIdle, StateLittleExcitedIdle, StateExcitedIdle };

        public int Mood
        {
            get => mood;
            set
            {
                if (mood != value)
                {
                    mood = value;
                    animator.Play(MoodIdleStates[mood], 0, 0f);
                }
            }
        }
        private int mood;

        void Awake()
        {
            animator = GetComponent<Animator>();
        }

        void Start()
        {
            animator.Play(StateIdle, 0, 0f);
            ScheduleNextBlink();
        }

        void Update()
        {
            // For mood 0: periodic blink
            // For mood 1/2: the looping idle clips handle animation automatically

            // After one-shot animations (Blink, Chew) finish, return to mood idle
            var state = animator.GetCurrentAnimatorStateInfo(0);
            if (state.normalizedTime >= 1f && !IsIdleState(state))
            {
                animator.Play(MoodIdleStates[mood], 0, 0f);
            }

            if (mood == 0 && Time.time >= nextBlinkTime)
            {
                animator.Play(StateBlink, 0, 0f);
                ScheduleNextBlink();
            }
        }

        bool IsIdleState(AnimatorStateInfo state)
        {
            return state.IsName(StateIdle) || state.IsName(StateLittleExcitedIdle) || state.IsName(StateExcitedIdle);
        }

        public void PlayChew()
        {
            animator.Play(StateChew, 0, 0f);
        }

        void ScheduleNextBlink()
        {
            nextBlinkTime = Time.time + Random.Range(blinkIntervalMin, blinkIntervalMax);
        }
    }
}
