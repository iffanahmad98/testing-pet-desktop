using Spine.Unity;

public static class AnimUtils
{
    public static void AddIdle(SkeletonGraphic anim) =>    
        anim.AnimationState.AddAnimation(0, "idle", true, 0);

    public static void SetIdle(SkeletonGraphic anim) =>
        anim.AnimationState.SetAnimation(0, "idle", true);    

    public static void AddAnim(SkeletonGraphic anim, string motion)=>
        anim.AnimationState.AddAnimation(0, motion, false, 0);

    public static void SetAnim(SkeletonGraphic anim, string motion) =>
        anim.AnimationState.SetAnimation(0, motion, false);
}
