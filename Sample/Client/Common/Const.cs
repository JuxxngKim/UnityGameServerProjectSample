using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace YeongJ
{
    public static class Const
    {
        public static float FrameTime = 0.1f;
        public static float MoveLerpDelay = 0.01f;

        public static int TriggerAttack = Animator.StringToHash("Attack");
        public static int TriggerSkill = Animator.StringToHash("Skill");
        public static int TriggerDance = Animator.StringToHash("Dance");
        public static int TriggerDeath = Animator.StringToHash("Death");
        public static int TriggerHit = Animator.StringToHash("Hit");
    }
}