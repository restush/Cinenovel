using Naninovel;
using UnityEditor;
using UnityEditor.Presets;
using UnityEngine;
using UnityEngine.Playables;

namespace AmoyFeels.Cinenovel.Editor
{
    public class CinenovelSettings : ResourcefulSettings<CinenovelConfiguration>
    {
        protected override System.Type ResourcesTypeConstraint => typeof(PlayableDirector);
        protected override string ResourcesCategoryId => Configuration.Loader.PathPrefix;
        public override void OnTitleBarGUI()
        {
            var boldStyle = EditorStyles.boldLabel;
            boldStyle.fontSize = 13;
            boldStyle.font = EditorStyles.miniBoldFont;
            if (GUI.Button(new Rect(120, 5, 180, 20), "(Timeline for Naninovel)", boldStyle))
                Application.OpenURL("https://docs.unity3d.com/Manual/com.unity.timeline.html");
        }
        protected override string HelpUri => base.HelpUri;
    }

}