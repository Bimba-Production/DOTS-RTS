﻿using UnityEditor;
using UnityEngine;

namespace _Scripts
{
    public class AnimationCreator : MonoBehaviour
    {
        [SerializeField] private Animator animator;
        [SerializeField] private SkinnedMeshRenderer skinnedMeshRenderer;
        [SerializeField] private int frameCount;
        [SerializeField] private float timePerFrame;
        [SerializeField] private string animationName = "Soldier_Idle";


        private void Start() {
            animator.Update(0f);

            for (int frame = 0; frame < frameCount; frame++) {
                Mesh mesh = new Mesh();
                skinnedMeshRenderer.BakeMesh(mesh);

                AssetDatabase.CreateAsset(mesh, "Assets/MeshBakeOutput/" + animationName + "_" + frame + ".asset");

                animator.Update(timePerFrame);
            }
        }
    }
}