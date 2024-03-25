using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LinePaint
{
    public class SoundManager_paint : MonoBehaviour
    {
        private static SoundManager_paint instance;

        [SerializeField] private AudioClip btnFx, brushMoveFx, victoryFx;
        [SerializeField] private AudioSource fxSource;

        public static SoundManager_paint Instance { get => instance; }

        private void Awake()
        {
            if (instance == null)
            {
                instance = this;
            }
            else
            {
                Destroy(gameObject);
            }
        }

        public void PlayFx(FxType fxType)
        {
            switch (fxType)
            {
                case FxType.Button:
                    fxSource.PlayOneShot(btnFx);
                    break;
                case FxType.BrushMove:
                    fxSource.PlayOneShot(brushMoveFx);
                    break;
                case FxType.Victory:
                    fxSource.PlayOneShot(victoryFx);
                    break;
            }
        }
    }

    public enum FxType
    {
        Button,
        BrushMove,
        Victory
    }
}