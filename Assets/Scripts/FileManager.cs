using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.Networking;
using System.IO; //IO, system.in and system.out
using System.Linq;
using UnityEngine.UI;
using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Text;
using Newtonsoft.Json.Linq; //Convert into JSON format
using System.Text.RegularExpressions; //Going to al;low us to use streamwriter 


public class FileManager : MonoBehaviour
{
    string path;
    public Text myText;
    public GameObject recallTextObject;
    public Transform contentwindow;
    //ScrollRect = Controlling our Scroll Bar, 1 to -1 End of our scroll
    public ScrollRect srollRect; //The variable that will control the position of our scrool view 
    // Start is called before the first frame update

    static string subscriptionKey = "07f984d4df974c16a3ff46cf79c542f4";
    static string endpoint = "https://erictextreaderapp.cognitiveservices.azure.com/";
    static string urlBase = endpoint + "vision/v2.1/ocr";

    public GameObject[] menuItems;
    public GameObject field;
    public GameObject readInputTextButton;

    public void inputText() //When we click our inputtext button it will run this code
    {
        //disable all the current buttons and gui elements
        foreach (GameObject i in menuItems)
        {
            i.SetActive(false);
        }
        //enable our text input gui 
        field.SetActive(true);
        readInputTextButton.SetActive(true);
    }

    public void readInputText() //When we clikc our readinputtext button it will run this code
    {
        //first get whatever we inputed into our text field
        string word = field.GetComponent<InputField>().text;
        //make a new list called words
        List<string> words = new List<string>();
        //we are going to use the split method to split up each line of words
        //we also use the list method to convert our string into a list to save it into words
        words = word.Split('\n').ToList();
        //this disables our Inputtext GUI
        field.SetActive(false);
        readInputTextButton.SetActive(false);
        //Enable all our previous menu items
        foreach (GameObject i in menuItems)
        {
            i.SetActive(true);
        }
        //Pass words into readText which will read whatever we inputted
        StartCoroutine(readText(words));
    }

    IEnumerator readText(List<string> fileLines) //fileLines is all the lines our file in a list 
    {
        float scrollPos = srollRect.verticalNormalizedPosition; //Getting our vertical scroll view Position
        float endPos = -1f; //Where we want to set our scrollPos everytime we add a text 
        string voice="-v Bruce "; //This is the voice we want to read in 
        GameObject textObj;
            //Print the time of when the function is first called.
        Debug.Log("Started Coroutine at timestamp : " + Time.time);
//use our Say method System.Diag
        //yield on a new YieldInstruction that waits for 5 seconds.

        //After we have waited 5 seconds print the time again.
        Debug.Log("Finished Coroutine at timestamp : " + Time.time);
        //This foreach loop, is looping through our fileLines (Which holds all our inputed text) line by line
        //Creating a text prefab for them and placing them in the scroll view
        //Then reads the text
        //
        foreach (string line in fileLines)
        {
            if (line != null) //Checking if a line is null, null is nothing if there is a blank line skip it 
            {
                //Print our text
                textObj = Instantiate(recallTextObject, contentwindow); //Creates a new prefab or text object, printed into our text view
                textObj.GetComponent<Text>().text = line;
                //Saying our line
                voice=voice+line;
                print(voice);
                System.Diagnostics.Process.Start("say", voice);
                //S
                srollRect.verticalNormalizedPosition = Mathf.Lerp(scrollPos, endPos, 3f);//scrollPos = start of our our scroll position
                                                                                    //endPos = where our scroll view will end up
                                                                                    //3f = how much time it will take to go from start to end
                yield return new WaitForSeconds(0.5f); //Rate at which he speaks each line
            }
          voice="-v Bruce ";
        }
    }

    public void OpenFileExplorer(string path)
    {
        
        //path = EditorUtility.OpenFilePanel("Show all files", "", ""); //Is going to contain the path to our file where it is on the computer

        if (path != "") //when is the path empty? When we don't choose a file

        {
            if (path.Substring(path.Length - 3) == "png") //checking if the file is a png 
            {
                Debug.Log("work");
            }
            if (path.Substring(path.Length - 3) == "txt") //checking if the file is a txt
            {
                List<string> fileLines = File.ReadAllLines(path).ToList(); //now after we get our path, we want to open the file and read the lines in it toList or toString? 

                StartCoroutine(readText(fileLines)); //We are using readText to read in the lines
                
            }

            else
            {
                Debug.Log("input a txt or png file");

            }
           // Debug.Log(text);
            //System.Diagnostics.Process.Start("say '#{text}'"));

        }

   
        
    }

    public void ResetText() 
    {
        Transform[] ts = contentwindow.GetComponentsInChildren<Transform>();
        foreach (Transform child in contentwindow.transform) {
            GameObject.Destroy(child.gameObject);
        }
    }

    public void OpenPNG(string path) { 
       //string path = EditorUtility.OpenFilePanel("Show all files", "", "");
       

        Main(path);
    }
    public async Task Main(string path){ 
        if (path!= ""){
            Debug.Log("begin");
            await extracttext(path); //await needs the fuction to be a aysnc, await WAITS for a function to return 

        }
    }
    public async Task extracttext(string path) {
        try //Try whatver is in here
        {
            HttpClient client = new HttpClient(); //making a new instance of HttpClient
            client.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", subscriptionKey); //Adding our AI to the client 
            string requestParams = "language=unk&dectectOrientation=true"; //Dectect the input langauge, and factor in oritentaion before reading
            string url = urlBase + "?" + requestParams;

            HttpResponseMessage response; //Hold the text file when the AI sends it back

            //This is making a byte array out of our photo so the AI can read each pixel of the photo
            byte[] dataBytes = File.ReadAllBytes(path);
            using (ByteArrayContent content = new ByteArrayContent(dataBytes)) 
            {
                content.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");
                response = await client.PostAsync(url,content);

            }
            string contentString = await response.Content.ReadAsStringAsync(); //Saving our response content into contentString

            string rss = JToken.Parse(contentString).ToString(); //Convert our content string into a formated JSON string
            List<string> words = new List<string>(); //Make a new array called words
            path = "Assets/test.txt"; //Sets up the path to our temporary text file
            StreamWriter writer = new StreamWriter(path, false); //Open our text file
            writer.WriteLine(rss); //Write our Formated Json String to our file this is beacuse we want convert it into an array
            writer.Close(); //Close the file to save changes
            List<string> fileLines = File.ReadAllLines(path).ToList(); //Convert our file to our lines array
            //Loop through  our line array 
            foreach(string line in fileLines)
            {
                if (line.Contains("\"text\":")) //FIind all the lines starting with text 
                {
                    string result = line.Replace("\"text\":", "").Replace("\"", ""); //Take out parts of the line we dont want
                    words.Add(result); //Add that word to the words array 
                }
            }
            StartCoroutine(readText(words));//pass our array readText Functions

        }
        catch (Exception e) { //if it breaks CATCH the error 
            Debug.Log(e);
        }
    }
}