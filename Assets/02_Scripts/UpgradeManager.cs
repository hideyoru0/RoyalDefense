using DG.Tweening;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class UpgradeManager : MonoBehaviour
{
    private static UpgradeManager _instance;

    public static UpgradeManager Instance()
    {
        return _instance;
    }

    public GameObject possiblePanel;
    public CanvasGroup levelUpText;
    public Text priceText;
    public Text levelText;

    public Text sellPrice;
    public Text buildingName;

    public Collider objectCollider;
    public int price;
    public int level;

    void Start()
    {
        if(_instance == null)
        {
            _instance = this;
        }
    }

    public void UpgradeClick(Collider _object)
    {
        UIManager.Instance().upgradePanel.SetActive(true);
        UIManager.Instance().upgradePanel.transform.DOScale(new Vector3(1f, 1f, 1f), 0.5f).SetEase(Ease.InExpo).SetEase(Ease.OutBounce);

        UIManager.Instance().mainPanel.SetActive(false);
        UIManager.Instance().pricePanel.SetActive(true);

        UIManager.Instance().GoldTextChnage();
        UIManager.Instance().DiaTextChange();

        if (objectCollider != null && objectCollider != _object)
        {
            objectCollider.transform.GetChild(1).gameObject.SetActive(false);
            objectCollider.transform.GetChild(3).gameObject.SetActive(false);
        }
        objectCollider = _object;
        objectCollider.transform.GetChild(1).gameObject.SetActive(true);
        objectCollider.transform.GetChild(3).gameObject.SetActive(true);

        if (objectCollider.CompareTag("Pawn"))
        {
            price = objectCollider.GetComponent<PawnController>().upgradePrice;
            level = objectCollider.GetComponent<PawnController>().level;

            priceText.text = price + " D";
            levelText.text = string.Format("LV.{0:00}", level);

            if (GameManager.Instance().diamond >= price)
            {
                possiblePanel.SetActive(false);
            }
            else
            {
                possiblePanel.SetActive(true);
            }
        }
        else
        {
            price = objectCollider.GetComponent<BuildingController>().upgradePrice;
            level = objectCollider.GetComponent<BuildingController>().level;

            priceText.text = price + " G";
            levelText.text = string.Format("LV.{0:00}", level);

            if (GameManager.Instance().gold >= price)
            {
                possiblePanel.SetActive(false);
            }
            else
            {
                possiblePanel.SetActive(true);
            }
        }

        if (level >= 5)
        {
            levelText.text = "MAX LV";
            priceText.text = "";
            possiblePanel.SetActive(true);
        }
    }

    public void UpgradeButton()
    {
        if (objectCollider.CompareTag("Pawn"))
        {
            GameManager.Instance().diamond -= price;
            objectCollider.GetComponent<PawnController>().LevelUP();
        }
        else
        {
            GameManager.Instance().gold -= price;
            objectCollider.GetComponent<BuildingController>().LevelUP();
        }

        UIManager.Instance().GoldTextChnage();
        UIManager.Instance().DiaTextChange();
        objectCollider.transform.GetChild(3).localScale += new Vector3(3.0f, 0.0f, 3.0f);

        UpgradeClick(objectCollider);
    }

    public void UpgradePanelClose()
    {
        UIManager.Instance().upgradePanel.transform.DOScale(new Vector3(0.05f, 0.05f, 0.05f), 0.15f).SetEase(Ease.InOutExpo).OnComplete(() => UIManager.Instance().upgradePanel.SetActive(false));

        if (objectCollider != null)
        {
            objectCollider.transform.GetChild(1).gameObject.SetActive(false);
            objectCollider.transform.GetChild(3).gameObject.SetActive(false);
        }
    }

    public void ObstacleCheck(Collider _object)
    {
        if (objectCollider != null && objectCollider != _object)
        {
            objectCollider.transform.GetChild(0).gameObject.SetActive(false);
        }
        objectCollider = _object;
        objectCollider.transform.GetChild(0).gameObject.SetActive(true);
    }

    public void ObstacleDelete()
    {
        UIManager.Instance().deletePanel.transform.DOScale(new Vector3(0.05f, 0.05f, 0.05f), 0.15f).SetEase(Ease.InOutExpo).OnComplete(() => UIManager.Instance().deletePanel.SetActive(false));
        GameManager.Instance().gold -= 4;

        if (objectCollider != null)
        {
            UIManager.Instance().GoldTextChnage();
            UIManager.Instance().DiaTextChange();
            objectCollider.transform.DOScale(new Vector3(0.1f, 0.1f, 0.1f), 0.15f).SetEase(Ease.InOutExpo).SetEase(Ease.OutBounce).OnComplete(() => Destroy(objectCollider.gameObject));
        }
    }

    public void ObstacleClose()
    {
        UIManager.Instance().deletePanel.transform.DOScale(new Vector3(0.05f, 0.05f, 0.05f), 0.15f).SetEase(Ease.InOutExpo).OnComplete(() => UIManager.Instance().deletePanel.SetActive(false));

        if (objectCollider != null)
        {
            objectCollider.transform.GetChild(0).gameObject.SetActive(false);
        }
    }

    public void BuildingSelect(Collider _building)
    {
        if (objectCollider != null && objectCollider != _building)
        {
            objectCollider.transform.GetChild(2).gameObject.SetActive(false);
        }
        objectCollider = _building;
        objectCollider.transform.GetChild(2).gameObject.SetActive(true);

        price = objectCollider.GetComponent<BuildingController>().sellPrice;
        buildingName.text = objectCollider.GetComponent<BuildingController>().buildingName;
        sellPrice.text =  price + " G";
    }

    public void BuildingSellButton()
    {
        GameManager.Instance().gold += price;

        if(objectCollider != null)
        {
            AudioManager.Instance().SoundPlay(AudioManager.Instance().coinSpawnSound);
            UIManager.Instance().GoldTextChnage();
            UIManager.Instance().DiaTextChange();

            objectCollider.GetComponent<BuildingController>().Sell();
            objectCollider.transform.DOScale(new Vector3(0.1f, 0.1f, 0.1f), 0.25f).SetEase(Ease.InOutExpo).SetEase(Ease.OutBounce);
            Destroy(objectCollider.gameObject, 0.15f);
        }
        else
        {
            AudioManager.Instance().SoundPlay(AudioManager.Instance().sellFailSound);
        }
    }

    public void BuildingSellClose()
    {
        if (objectCollider != null)
        {
            objectCollider.transform.GetChild(2).gameObject.SetActive(false);
        }
    }

    public IEnumerator LevelUPPanel()
    {
        levelUpText.gameObject.SetActive(true);
        levelUpText.alpha = 1;
        levelUpText.transform.DOScale(new Vector3(1f, 1f, 1f), 0.65f).SetEase(Ease.InExpo).SetEase(Ease.OutBounce);
        yield return new WaitForSeconds(2.5f);

        levelUpText.alpha = 1;
        Tween tween = levelUpText.DOFade(0f, 1f);
        yield return tween.WaitForCompletion();
        levelUpText.transform.DOScale(new Vector3(0.05f, 0.05f, 0.05f), 0.05f).OnComplete(() => levelUpText.gameObject.SetActive(false));
    }

    public void InitObject()
    {
        price = 0;
        objectCollider = null;
    }
}
