using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using ScatterSharp;
using EosSharp.Api.v1;
using Newtonsoft.Json;
using ScatterSharp.Storage;
using System.Linq;
using System;

public class clickController : MonoBehaviour
{
    // Start is called before the first frame update
    public List<GameObject> choice_cards;
    public List<GameObject> color_cards;
    public List<GameObject> chips;
    public Canvas chipCanvas;

    public Text resultText;
    public Text howMuchYouWin;
    public GameObject chipPre;

    private List<GameObject> chipsOnCards;

    private int card_int = 0;

    private GameObject selected_chip;
    private void Start()
    {
        chipsOnCards = new List<GameObject>();
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Vector2 mousePos2D = new Vector2(mousePos.x, mousePos.y);

            RaycastHit2D hit = Physics2D.Raycast(mousePos2D, Vector2.zero);
            Debug.Log("clicked");
            if (hit.collider != null)
            {
                if (hit.collider.gameObject.CompareTag("choice_card"))
                {
                    Image img = hit.transform.Find("Image").gameObject.GetComponent<Image>();
                    if (selected_chip == null)
                    {
                        StartCoroutine(FlashRed(0.2f,img));
                    }
                    else
                    {
                        

                        double selected_value = selected_chip.GetComponent<chip_prop>().bet_value;
                        hit.transform.gameObject.GetComponent<choice_card_prop>().bet_value += selected_value;
                        Text bet_text = hit.transform.Find("betInfo").gameObject.GetComponent<Text>();
                        bet_text.text = "Bet : " + hit.transform.gameObject.GetComponent<choice_card_prop>().bet_value.ToString();


                        Vector3 endloc = hit.transform.position;

                        GameObject OneChip = Instantiate(chipPre, selected_chip.transform.position, Quaternion.identity);
                        OneChip.transform.parent = chipCanvas.transform;
                        OneChip.transform.localScale = new Vector3(20f, 20f, 1f);
                        OneChip.transform.gameObject.GetComponent<betChip_prop>().choice_card = hit.transform.gameObject;
                        OneChip.transform.gameObject.GetComponent<betChip_prop>().chip_value = selected_value;

                        chipsOnCards.Add(OneChip);

                        StartCoroutine(MoveOverSeconds(OneChip, endloc, 0.5f));
                    }

                }
                if (hit.collider.gameObject.CompareTag("chip"))
                {
                    ResetList(chips);
                    SpriteRenderer sr = hit.transform.Find("Image").gameObject.GetComponent<SpriteRenderer>();
                    if (sr != null)
                    {
                        sr.color = Color.yellow;
                    }
                    selected_chip = hit.transform.gameObject;

                    
                }
            }
        }
    }
    private void ResetList(List<GameObject> glist)
    {
        foreach(GameObject obj in glist)
        {
            SpriteRenderer sr = obj.transform.Find("Image").gameObject.GetComponent<SpriteRenderer>();
            if (sr != null)
            {
                sr.color = Color.white;
            }

        }
    }

    public void OnClickBet()
    {
        int card = UnityEngine.Random.Range(1, 53);
        resultText.text = CardNumToStr(card);
        // PushTransaction();
        double res = checkResult(card);
        howMuchYouWin.text = "how much you win :" + res.ToString();
    }

    private double checkResult(int card)
    {
        double res = 0;
        foreach(GameObject chip in chipsOnCards)
        {
            GameObject choice_card = chip.GetComponent<betChip_prop>().choice_card;
            double chip_value = chip.GetComponent<betChip_prop>().chip_value;
            double card_value = choice_card.GetComponent<choice_card_prop>().card_value;
            int winrate = choice_card.GetComponent<choice_card_prop>().bet_rate;
            if (card_value > 0)
            {
                if(card_value == 53 && card == 53)
                {
                    res += chip_value * winrate;
                }else if(card_value == card%13){
                    res += chip_value * winrate;
                }
            }
            else
            {
                if(card_value == -1 && card%2 == 1 && card!=53)
                {
                    res += chip_value * winrate;
                }
                if(card_value == -2 && card%2 == 0)
                {
                    res += chip_value * winrate;
                }
                if(card_value == -3 && card%4 == 0)
                {
                    res += chip_value * winrate;
                }
                if (card_value == -4 && card % 4 == 1 && card!= 53)
                {
                    res += chip_value * winrate;
                }
                if (card_value == -5 && card % 4 == 2)
                {
                    res += chip_value * winrate;
                }
                if (card_value == -6 && card % 4 == 3)
                {
                    res += chip_value * winrate;
                }

            }
        }
        return res;
    }
    private string CardNumToStr(int card)
    {
        string res = "";
        if(card%4 == 0)
        {
            res += "heart ";
        }
        if(card%4 == 1)
        {
            res += "spade ";
        }
        if (card % 4 == 2)
        {
            res += "diamond ";
        }
        if (card % 4 == 3)
        {
            res += "club ";
        }
        if(card % 13 == 0)
        {
            res += "K";
        }
        else if(card % 13 == 1)
        {
            res += "A";
        }
        else if(card % 13 == 11)
        {
            res += "J";
        }
        else if (card % 13 == 12)
        {
            res += "Q";
        }
        else
        {
            res += (card % 13).ToString();
        }


        return res;
    }

    public void OnClickClear()
    {
        foreach(GameObject obj in chipsOnCards){
            GameObject card = obj.GetComponent<betChip_prop>().choice_card;
            card.GetComponent<choice_card_prop>().bet_value = 0;
            card.transform.Find("betInfo").gameObject.GetComponent<Text>().text = "";
            Destroy(obj);
        }
        chipsOnCards.Clear();
        howMuchYouWin.text = "";
        resultText.text = "";
    }

    public void OnclickCancel()
    {
        if (chipsOnCards.Count != 0)
        {
            print(chipsOnCards.Count);
            GameObject chip = chipsOnCards[chipsOnCards.Count - 1];
            GameObject card = chip.GetComponent<betChip_prop>().choice_card;
            card.GetComponent<choice_card_prop>().bet_value -= chip.GetComponent<betChip_prop>().chip_value;
            if (card.GetComponent<choice_card_prop>().bet_value == 0)
            {
                card.transform.Find("betInfo").gameObject.GetComponent<Text>().text = "";
            }
            else
            {
                card.transform.Find("betInfo").gameObject.GetComponent<Text>().text = "Bet : " + card.GetComponent<choice_card_prop>().bet_value.ToString();
            }

            Destroy(chipsOnCards[chipsOnCards.Count - 1]);
            chipsOnCards.RemoveAt(chipsOnCards.Count - 1);
        }
        
    }


    
    IEnumerator FlashRed(float stime,Image img)
    {
        Color imgcolor = img.color;
        img.color = Color.red;
        yield return new WaitForSeconds(stime);
        img.color = imgcolor;
    }

    public IEnumerator MoveOverSeconds(GameObject objectToMove, Vector3 end, float seconds)
    {
        float elapsedTime = 0;
        Vector3 startingPos = objectToMove.transform.position;
        while (elapsedTime < seconds)
        {
            objectToMove.transform.position = Vector3.Lerp(startingPos, end, (elapsedTime / seconds));
            elapsedTime += Time.deltaTime;
            yield return new WaitForEndOfFrame();
        }
        objectToMove.transform.position = end;
    }

    private double chipTextToDouble(string chipText)
    {
        string num = chipText.Split(null)[0];
        return Convert.ToDouble(num);
    }



    public async void PushTransaction()
    {
        try
        {
            var network = new ScatterSharp.Api.Network()
            {
                Blockchain = Scatter.Blockchains.EOSIO,
                Host = "eos.greymass.com",
                Port = 443,
                Protocol = "https",
                ChainId = "aca376f206b8fc25a6ed44dbdc66547c36c6c33e3a119ffbeaef943642f0e906"
            };

            var fileStorage = new FileStorageProvider(Application.persistentDataPath + "/scatterapp.dat");

            using (var scatter = new Scatter("dicePoker", network, fileStorage))
            {
                await scatter.Connect();

                await scatter.GetIdentity(new ScatterSharp.Api.IdentityRequiredFields()
                {
                    Accounts = new List<ScatterSharp.Api.Network>()
                    {
                        network
                    },
                    Location = new List<ScatterSharp.Api.LocationFields>(),
                    Personal = new List<ScatterSharp.Api.PersonalFields>()
                });

                print(scatter.GetVersion());

                var eos = scatter.Eos();

                var account = scatter.Identity.Accounts.First();

                var result = await eos.CreateTransaction(new Transaction()
                {
                    Actions = new List<EosSharp.Api.v1.Action>()
                    {
                        new EosSharp.Api.v1.Action()
                        {
                            Account = "eosio.token",
                            Authorization =  new List<PermissionLevel>()
                            {
                                new PermissionLevel() {Actor = account.Name, Permission = account.Authority }
                            },
                            Name = "transfer",
                            Data = new { from = account.Name, to = "eosvegasjack", quantity = "0.0001 EOS", memo = "Unity 3D test from blockfish!" }
                        }
                    }
                });

                print(result);
            }
        }

        catch (Exception ex)
        {
            print(JsonConvert.SerializeObject(ex));
        }
    }

}
