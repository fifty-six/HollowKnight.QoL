using System.Reflection;
using TMPro;
using Modding;

namespace FastText
{
    public class FastText : Mod
    {
        public override void Initialize()
        {
            FieldInfo textMesh = typeof(DialogueBox).GetField("textMesh", BindingFlags.NonPublic | BindingFlags.Instance);
            On.DialogueBox.ShowNextChar += (orig, self) =>
            {
                TextMeshPro text = (TextMeshPro)textMesh.GetValue(self);
                text.maxVisibleCharacters = text.textInfo.pageInfo[self.currentPage - 1].lastCharacterIndex + 1;
            };
        }
    }
}
