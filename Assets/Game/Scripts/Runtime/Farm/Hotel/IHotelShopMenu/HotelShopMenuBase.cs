using UnityEngine;
using UnityEngine.UI;
public class HotelShopMenuBase : MonoBehaviour, IHotelShopMenu
{
   public Sprite onTitleButtonSprite;
   public Sprite offTitleButtonSprite;

   public Button titleButtonSprite;
   public Image mainBackground;

   public virtual void OnTitleButtonSprite () {
    titleButtonSprite.image.sprite = onTitleButtonSprite;
   }

   public virtual void OffTitleButtonSprite () {
    titleButtonSprite.image.sprite = offTitleButtonSprite;
   }

   public virtual void ShowMenu () {
      OnTitleButtonSprite ();
      mainBackground.gameObject.SetActive (true);
      Debug.Log ("Show hotel shop");
   }

   public virtual void HideMenu () {
      OffTitleButtonSprite ();
      mainBackground.gameObject.SetActive (false);
      Debug.Log ("Hide hotel shop");
   }
}
