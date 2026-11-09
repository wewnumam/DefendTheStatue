using NaughtyAttributes;
using System;
using UnityEngine;

namespace ProjectTA.Module.Tutorial
{
    [Serializable]
    public class TutorialItem
    {
        [ShowAssetPreview]
        [SerializeField] private Sprite _image;
        [SerializeField] private string _title;
        [TextArea]
        [SerializeField] private string _description;

        public Sprite Image => _image;
        public string Title => _title;
        public string Description => _description;
    }
}