using System;
using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class ImportNFTIPFS : MonoBehaviour
{
    MainLobbyPanel mLobbyPanel;
    public List<NftState> nftStates;
    static bool flag = false;

    private class NFTs
    {
        public string contract { get; set; }

        public string tokenId { get; set; }

        public string uri { get; set; }

        public string balance { get; set; }
    }

    public class Attribute
    {
        public string trait_type { get; set; }

        public string value { get; set; }
    }

    public class Response
    {
        public string image { get; set; }

        public string name { get; set; }

        public string nftimg { get; set; }

        public List<Attribute> attributes { get; set; }
    }

    public class NftState
    {
        public string NFTid;

        public string name;

        public string cost;

        public string atk;

        public string hp;

        public string image;

        public string nftimg;
    }

    public void Start()
    {
        nftStates = new List<NftState>();
        mLobbyPanel = gameObject.GetComponentInChildren<MainLobbyPanel>();
        LoadNftData();
    }

    public async void LoadNftData()
    {
        if (flag) return;
        flag = true;

        foreach (Button btn in mLobbyPanel.gameObject.GetComponentsInChildren<Button>())
        {
            btn.interactable = false;
        }

        string chain = "polygon";
        string network = "testnet";

        // BAYC contract address
        string contract = "0xf86E1e27FaD42C7B965Be0860a44431fdcF8EEE1";

        string account = "0x9592C551eDCf5b83bb80C656c5c121B3A9aC5B00";
        //string account = PlayerPrefs.GetString("Account");

        List<string> tokenIds = new List<string>();
        List<string> uris = new List<string>();
        int first = 500;
        int skip = 0;
        string response = await EVM.AllErc721(chain, network, account, contract, first, skip);

        NFTs[] erc721s = JsonConvert.DeserializeObject<NFTs[]>(response);
        for (int i = 0; i < erc721s.Length; i++)
            tokenIds.Add(erc721s[i].tokenId);
        

        for (int i = 0; i < tokenIds.Count; i++)
        {
           
            // fetch uri from chain
            string uri = await ERC721.URI(chain, network, contract, tokenIds[i]);
            
            
            //uri = string.Empty;
            if (uri.StartsWith("Qm"))
                uri = uri.Replace("Qm", "https://ipfs.io/ipfs/Qm");
            
            uris.Add (uri);
            uri = string.Empty;
  
            // fetch json from uri
            UnityWebRequest webRequest = UnityWebRequest.Get(uris[i]);
            await webRequest.SendWebRequest();
            Response data =
                JsonConvert
                    .DeserializeObject<Response>(System
                        .Text
                        .Encoding
                        .UTF8
                        .GetString(webRequest.downloadHandler.data));

            NftState temp = new NftState();

            temp.NFTid = tokenIds[i].ToString();
            temp.name = data.name;
            temp.cost = data.attributes[0].value;
            temp.atk = data.attributes[1].value;
            temp.hp = data.attributes[2].value;
 
            nftStates.Add (temp);
            Debug.Log (temp.NFTid);
        }

        Managers.Data.MakeNftDict(nftStates);
        foreach (Button btn in mLobbyPanel.gameObject.GetComponentsInChildren<Button>())
        {
            btn.interactable = true;
        }

    }


}
