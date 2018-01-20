using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using System.IO;
using Hotfire;

public class ParticleSystemBatchEditor : EditorWindow
{
    string[] m_arr_Root_Folder_Path;
    readonly string[] EFFECT_ROOT_PREF_KEY = new string[] {
        "Effect_Prefab_Folder_Path",
        "Effect_Material_Folder_Path",
        "Effect_Texture_Folder_Path"
    };

    readonly string[] DEFULT_ROOT_PATH = new string[]{
        "Resources/Prefabs/Effect",
        "Resources/ParticleSystemBatch/Material",
        "Resources/ParticleSystemBatch/Texture"
    };

    //[MenuItem(EditorConfig.customToolsPath + "ParticleSystemBatch")]
    public static void ShowWindow()
    {
        EditorWindow.GetWindow(typeof(ParticleSystemBatchEditor));

    }


    void OnEnable()
    {
        int iLength = EFFECT_ROOT_PREF_KEY.Length;
        if (m_arr_Root_Folder_Path == null)
        {
            m_arr_Root_Folder_Path = new string[iLength];
        }
        for (int i = 0; i < EFFECT_ROOT_PREF_KEY.Length; i++)
        {
            m_arr_Root_Folder_Path[i] = EditorPrefs.GetString(EFFECT_ROOT_PREF_KEY[i], Application.dataPath + "/" + DEFULT_ROOT_PATH[i]);
        }
    }

    void OnGUI()
    {
        EditorGUILayout.BeginVertical();

        for (int i = 0; i < EFFECT_ROOT_PREF_KEY.Length; i++)
        {
            GUIStyle kGUIStyle = new GUIStyle();
            kGUIStyle.fixedHeight = 50;
            kGUIStyle.alignment = TextAnchor.MiddleCenter;

            Rect r = EditorGUILayout.BeginHorizontal(kGUIStyle);
            if (GUI.Button(r, GUIContent.none))
            {
                string chooseFolderPath = EditorUtility.OpenFolderPanel("Choose folder", "", "");
                if (!string.IsNullOrEmpty(chooseFolderPath))
                {
                    m_arr_Root_Folder_Path[i] = chooseFolderPath;
                    EditorPrefs.SetString(EFFECT_ROOT_PREF_KEY[i], m_arr_Root_Folder_Path[i]);
                }
            }
            GUILayout.Label(EFFECT_ROOT_PREF_KEY[i]);
            GUILayout.Label(m_arr_Root_Folder_Path[i]);
            EditorGUILayout.EndHorizontal();
        }

        if (GUILayout.Button("Batch"))
        {
            GoThroughPrefabsNew();
        }

        if (GUILayout.Button("DisBatch"))
        {
            SplitImages();
        }

        EditorGUILayout.EndVertical();
    }

    List<string> fileList = new List<string>();
    List<string> effectInfoList = new List<string>();

    void FindAllFilesInFolder(DirectoryInfo kFolderInfo)
    {
        if (kFolderInfo != null)
        {
            foreach (FileInfo kFileInfo in kFolderInfo.GetFiles())
            {
                fileList.Add(kFileInfo.FullName);
            }
            foreach (DirectoryInfo kDirectoryInfo in kFolderInfo.GetDirectories())
            {
                FindAllFilesInFolder(kDirectoryInfo);
            }
        }
    }

    //粒子路径通过读表确定，不合并路径下的非表中粒子
    void GoThroughPrefabsNew()
    {
        string relativePath = FileUtil.GetProjectRelativePath(m_arr_Root_Folder_Path[0]);
        //先看下之前的特效文件夹.
        string prefabFolderName = relativePath.Substring(relativePath.LastIndexOf("/") + 1);
        string dataPath = Application.dataPath;
        dataPath = dataPath.Substring(0, dataPath.Length - 6) + "EffectSingleImages/" + prefabFolderName + "/";
        if (Directory.Exists(dataPath))
        {
            Directory.Delete(dataPath, true);
        }
        Directory.CreateDirectory(dataPath);

        string strAllCode = "";
        List<string> kListAddedImages = new List<string>();
        fileList.Clear();
        FindAllFilesInFolder(new DirectoryInfo(m_arr_Root_Folder_Path[0]));



        List<string> arrContainShaderList = new List<string>();
        int index = 0;
        //TableManager.CreateAndLoad();
        //Dictionary<int, TableEffectCfg> allGameEffects = TableManager.instance.GetAllData<TableEffectCfg>();
        //foreach (var key in allGameEffects.Keys)
        //{
        //    EditorUtility.DisplayProgressBar("Go Through Prefabs", allGameEffects[key].name, (index + 1) * 1f / allGameEffects.Count);

        //    string prefabPath = allGameEffects[key].prefabPath;
        //    string fullPath = "Assets/Resources/" + prefabPath + ".prefab";
        //    GameObject go = AssetDatabase.LoadAssetAtPath<GameObject>(fullPath);
        //    if (go != null)
        //    {
        //        foreach (ParticleSystem mParticleSystem in go.GetComponentsInChildren<ParticleSystem>(true))
        //        {
        //            ParticleSystem.TextureSheetAnimationModule kModule = mParticleSystem.textureSheetAnimation;
        //            if (!kModule.enabled)
        //            {
        //                ParticleSystemRenderer kParticleRender = (ParticleSystemRenderer)mParticleSystem.GetComponent<Renderer>();
        //                if (kParticleRender != null)
        //                {
        //                    Material kMaterial = kParticleRender.sharedMaterial;
        //                    if (kMaterial != null)
        //                    {
        //                        string materialPath = AssetDatabase.GetAssetPath(kMaterial.GetInstanceID());
        //                        //Debug.Log("materialPath is "+ materialPath);
        //                        if (kMaterial != null)
        //                        {
        //                            Shader kShader = kMaterial.shader;
        //                            Texture kTexture = kMaterial.mainTexture;
        //                            if (kMaterial.HasProperty("_SecondTex")|| kMaterial.HasProperty("_MaskTex"))
        //                                continue;
        //                            if (kTexture != null)
        //                            {
        //                                string path = AssetDatabase.GetAssetPath(kTexture.GetInstanceID());
        //                                TextureImporter mainTextureImporter = AssetImporter.GetAtPath(path) as TextureImporter;
        //                                if (path.Equals("Resources/unity_builtin_extra"))
        //                                    continue;
        //                                if (mainTextureImporter != null)
        //                                {
        //                                    if (mainTextureImporter.isReadable == false)
        //                                    {
        //                                        mainTextureImporter.isReadable = true;
        //                                        AssetDatabase.ImportAsset(path);
        //                                    }
        //                                }

        //                                if (kShader != null)
        //                                {
        //                                    if (!arrContainShaderList.Contains(kShader.name))
        //                                    {
        //                                        arrContainShaderList.Add(kShader.name);
        //                                    }
        //                                }

        //                                strAllCode += "Prefab path:" + fullPath + " , Material path:" + materialPath + "," + " Shader name is:" + kShader.name + "," + "Texture path:" + path + "\n";
        //                                AssetDatabase.CopyAsset(path, "EffectSingleImages/" + prefabFolderName + "/" + path.Substring(path.LastIndexOf("/")));
        //                                if (!kListAddedImages.Contains(path))
        //                                {
        //                                    kListAddedImages.Add(path);
        //                                }
        //                            }
        //                        }
        //                    }
        //                }
        //            }
        //        }
        //    }
        //    index++;
        //}
        File.WriteAllText("EffectSingleImages/" + prefabFolderName + "/" + "EffectInfo.txt", strAllCode);
        CreateTotalImageAndMaterial(kListAddedImages, arrContainShaderList);
        EditorUtility.ClearProgressBar();
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
    }


    void GoThroughPrefabs()
    {
        string relativePath = FileUtil.GetProjectRelativePath(m_arr_Root_Folder_Path[0]);
        //先看下之前的特效文件夹.
        string prefabFolderName = relativePath.Substring(relativePath.LastIndexOf("/") + 1);
        string dataPath = Application.dataPath;
        dataPath = dataPath.Substring(0, dataPath.Length - 6) + "EffectSingleImages/" + prefabFolderName + "/";
        if (Directory.Exists(dataPath))
        {
            Directory.Delete(dataPath, true);
        }
        Directory.CreateDirectory(dataPath);

        string strAllCode = "";
        List<string> kListAddedImages = new List<string>();
        fileList.Clear();
        FindAllFilesInFolder(new DirectoryInfo(m_arr_Root_Folder_Path[0]));

        List<string> arrContainShaderList = new List<string>();
        int index = 0;
        foreach (string filePath in fileList)
        {
            if (!filePath.Contains(".meta"))
            {
                string temp = filePath.Replace("\\", "/");
                EditorUtility.DisplayProgressBar("Go Through Prefabs", temp.Substring(temp.LastIndexOf("/") + 1), (index + 1) * 1f / fileList.Count);
                string prefabPath = FileUtil.GetProjectRelativePath(temp);
                GameObject go = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath) as GameObject;
                if (go != null)
                {
                    foreach (ParticleSystem mParticleSystem in go.GetComponentsInChildren<ParticleSystem>(true))
                    {
                        ParticleSystem.TextureSheetAnimationModule kModule = mParticleSystem.textureSheetAnimation;
                        if (!kModule.enabled)
                        {
                            ParticleSystemRenderer kParticleRender = (ParticleSystemRenderer)mParticleSystem.GetComponent<Renderer>();
                            if (kParticleRender != null)
                            {
                                Material kMaterial = kParticleRender.sharedMaterial;
                                if (kMaterial != null)
                                {
                                    string materialPath = AssetDatabase.GetAssetPath(kMaterial.GetInstanceID());
                                    //Debug.Log("materialPath is "+ materialPath);
                                    if (kMaterial != null)
                                    {
                                        Shader kShader = kMaterial.shader;
                                        Texture kTexture = kMaterial.mainTexture;
                                        if (kMaterial.HasProperty("a"))
                                            ;

                                        if (kTexture != null)
                                        {
                                            string path = AssetDatabase.GetAssetPath(kTexture.GetInstanceID());
                                            TextureImporter mainTextureImporter = AssetImporter.GetAtPath(path) as TextureImporter;
                                            if (path.Equals("Resources/unity_builtin_extra"))
                                                continue;
                                            if (mainTextureImporter != null)
                                            {
                                                if (mainTextureImporter.isReadable == false)
                                                {
                                                    mainTextureImporter.isReadable = true;
                                                    AssetDatabase.ImportAsset(path);
                                                }
                                            }

                                            if (kShader != null)
                                            {
                                                if (!arrContainShaderList.Contains(kShader.name))
                                                {
                                                    arrContainShaderList.Add(kShader.name);
                                                }
                                            }

                                            strAllCode += "Prefab path:" + prefabPath + ",Material path:" + materialPath + "," + " Shader name is:" + kShader.name + "," + "Texture path:" + path + "\n";
                                            AssetDatabase.CopyAsset(path, "EffectSingleImages/" + prefabFolderName + "/" + path.Substring(path.LastIndexOf("/")));
                                            if (!kListAddedImages.Contains(path))
                                            {
                                                kListAddedImages.Add(path);
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }

            index++;
        }
        File.WriteAllText("EffectSingleImages/" + prefabFolderName + "/" + "EffectInfo.txt", strAllCode);

        CreateTotalImageAndMaterial(kListAddedImages, arrContainShaderList);

        EditorUtility.ClearProgressBar();
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
    }



    void SplitImages()
    {
        string relativePath = FileUtil.GetProjectRelativePath(m_arr_Root_Folder_Path[0]);
        fileList.Clear();
        FindAllFilesInFolder(new DirectoryInfo(m_arr_Root_Folder_Path[0]));
        InitEffectInfoList();

        int index = 0;
        foreach (string filePath in fileList)
        {
            if (!filePath.Contains(".meta"))
            {
                string temp = filePath.Replace("\\", "/");
                EditorUtility.DisplayProgressBar("Split All Prefabs", temp.Substring(temp.LastIndexOf("/") + 1), (index + 1) * 1f / fileList.Count);
                string prefabPath = FileUtil.GetProjectRelativePath(temp);
                GameObject go = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath) as GameObject;
                if (go != null)
                {
                    string kParticleSystemPath = AssetDatabase.GetAssetPath(go);
                    foreach (ParticleSystem mParticleSystem in go.GetComponentsInChildren<ParticleSystem>(true))
                    {
                        ParticleSystem.TextureSheetAnimationModule kModule = mParticleSystem.textureSheetAnimation;
                        if (kModule.enabled)
                        {
                            ParticleSystemRenderer kParticleRender = (ParticleSystemRenderer)mParticleSystem.GetComponent<Renderer>();
                            if (kParticleRender != null)
                            {
                                Material kMaterial = kParticleRender.sharedMaterial;
                                string materialPath = AssetDatabase.GetAssetPath(kMaterial.GetInstanceID());
                                if (kMaterial != null)
                                {
                                    Texture kTexture = kMaterial.mainTexture;
                                    if (kTexture != null)
                                    {
                                        string path = AssetDatabase.GetAssetPath(kTexture.GetInstanceID());
                                        string strImageName = path.Substring(path.LastIndexOf("/") + 1);
                                        if (strImageName.Contains("TotalImage"))
                                        {
                                            //根据x，y，z，w找到对应的图片.
                                            string strFileName = FindImageNameBySheetInfo(kModule.numTilesX, kModule.numTilesY, kModule.rowIndex, Mathf.RoundToInt(kModule.numTilesX * kModule.frameOverTime.constantMax));
                                            string shaderName = kMaterial.shader.name;
                                            if (shaderName.Contains("Split"))
                                            {
                                                shaderName = shaderName.Substring(0, shaderName.IndexOf("Split"));
                                            }
                                            Material kSubMaterial = GetOriginalOrCreateMaterial(strFileName, shaderName);
                                            kParticleRender.material = kSubMaterial;
                                            kModule.enabled = false;
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        EditorUtility.ClearProgressBar();
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
    }


    public Dictionary<MaterialInfo, string> materialPathDic = new Dictionary<MaterialInfo, string>();
    void InitEffectInfoList()
    {
        materialPathDic.Clear();
        string relativePath = FileUtil.GetProjectRelativePath(m_arr_Root_Folder_Path[0]);
        string prefabFolderName = relativePath.Substring(relativePath.LastIndexOf("/") + 1);
        string imageSheetInfoFilePath = "EffectSingleImages/" + prefabFolderName + "/" + "EffectInfo.txt";
        string AllSheetCode = File.ReadAllText(imageSheetInfoFilePath);
        string[] tmp = AllSheetCode.Split('\n');
        foreach (string str in tmp) {
            string[] infoSplits = str.Split(':', ',');
            if (infoSplits.Length < 8)
                continue;
            MaterialInfo info = new MaterialInfo();
            info.shaderName = infoSplits[5];
            info.texturePath = infoSplits[7];
            materialPathDic[info] = infoSplits[3];
            //str.Substring(str.IndexOf())
        }
    }

    public class MaterialInfo
    {
        public string shaderName;
        public string texturePath;
    }

    Material GetOriginalOrCreateMaterial(string fileAssetPath, string shaderName)
    {
        foreach (MaterialInfo info in materialPathDic.Keys)
        {
            if (info.texturePath.Equals(fileAssetPath) && info.shaderName.Equals(shaderName))
            {
                Material material = AssetDatabase.LoadAssetAtPath<Material>(materialPathDic[info]);
                if (material != null)
                    return material;
            }
        }
        return GetOrCreateMaterial(fileAssetPath, shaderName);
    }

    Material GetOrCreateMaterial(string fileAssetPath, string shaderName)
    {
        Material kMat = new Material(Shader.Find(shaderName));
        Texture kDesTexture = AssetDatabase.LoadAssetAtPath<Texture>(fileAssetPath);
        kMat.mainTexture = kDesTexture;

        string strImageName = fileAssetPath.Substring(fileAssetPath.LastIndexOf("/") + 1);
        strImageName = strImageName.Substring(0, strImageName.LastIndexOf("."));
        string matFileName = strImageName + "_" + shaderName.Replace('/', '_') + ".mat";
        string matPath = FileUtil.GetProjectRelativePath(m_arr_Root_Folder_Path[1]) + "/" + matFileName;
        if (AssetDatabase.LoadAssetAtPath<Material>(matPath) == null)
        {
            AssetDatabase.CreateAsset(kMat, matPath);
        }

        return AssetDatabase.LoadAssetAtPath<Material>(matPath);
    }

    string FindImageNameBySheetInfo(int x, int y, int z, int w)
    {
        string imageName = "";
        string imageSheetInfoFilePath = FileUtil.GetProjectRelativePath(m_arr_Root_Folder_Path[0]) + "/" + "ImageSheetInfo.txt";
        if (File.Exists(imageSheetInfoFilePath))
        {
            string AllSheetCode = File.ReadAllText(imageSheetInfoFilePath);
            string[] tmp = AllSheetCode.Split('\n');
            for (int i = 0; i < tmp.Length; i++)
            {
                if (tmp[i].Contains("TextSheet x: " + x + ", y: " + y + ", rowIndex: " + z + ", colIndex: " + w))
                {
                    return getImageAssetPath(tmp[i]);
                }
            }
        }

        return "";
    }

    string getImageAssetPath(string line)
    {
        int strFirstMaoHaoIndex = line.IndexOf(": ");
        int strFirstDouHaoIndex = line.IndexOf(",");
        string strFileName = line.Substring(strFirstMaoHaoIndex + 2, strFirstDouHaoIndex - strFirstMaoHaoIndex - 2);
        return strFileName;
    }

    void CreateTotalImageAndMaterial(List<string> images, List<string> arrShaders)
    {
        List<Texture2D> list = new List<Texture2D>();
        for (int i = 0; i < images.Count; i++)
        {
            Texture2D kTexture = AssetDatabase.LoadAssetAtPath<Texture2D>(images[i]);
            if (kTexture != null)
            {
                list.Add(kTexture);
            }
        }

        string strAllSheetInfo = "";
        List<List<ColorInfo>> kInfo = CustomTexturePacker.CombineTexture(list, ref strAllSheetInfo);
        File.WriteAllText(FileUtil.GetProjectRelativePath(m_arr_Root_Folder_Path[0]) + "/" + "ImageSheetInfo.txt", strAllSheetInfo);

        Texture2D kTotalImage = new Texture2D(kInfo.Count, kInfo.Count, TextureFormat.RGBA32, false);
        for (int i = 0; i < kInfo.Count; i++)
        {
            for (int j = 0; j < kInfo.Count; j++)
            {
                kTotalImage.SetPixel(i, kInfo.Count - j, kInfo[i][j].m_iColor);
            }
        }
        string relativeTextureFolderPath = FileUtil.GetProjectRelativePath(m_arr_Root_Folder_Path[2]);
        string relativePath = FileUtil.GetProjectRelativePath(m_arr_Root_Folder_Path[0]);
        string prefabFolderName = relativePath.Substring(relativePath.LastIndexOf("/") + 1);
        byte[] kTotalImageData = kTotalImage.EncodeToPNG();
        string saveTotalImageFilePath = relativeTextureFolderPath + "/TotalImage_" + prefabFolderName + ".png";
        string loadTotalImageFilePath = relativeTextureFolderPath + "/TotalImage_" + prefabFolderName;
        if (kTotalImageData != null)
        {
            File.WriteAllBytes(saveTotalImageFilePath, kTotalImageData);
        }
        AssetDatabase.Refresh();
        //Create TotalImage material.
        Dictionary<string, Material> dicTotalImageMat = new Dictionary<string, Material>();
        for (int i = 0; i < arrShaders.Count; i++)
        {
            EditorUtility.DisplayProgressBar("Creating TotalImage Material", "", (i + 1f) / arrShaders.Count);
            Material kMat = new Material(Shader.Find(arrShaders[i]));
            Texture kDesTexture = AssetDatabase.LoadAssetAtPath<Texture>(saveTotalImageFilePath);
            kMat.mainTexture = kDesTexture;

            string matFileName = "TotalImage_" + prefabFolderName + "_" + arrShaders[i].Replace('/', '_') + ".mat";
            string matPath = FileUtil.GetProjectRelativePath(m_arr_Root_Folder_Path[1]) + "/" + matFileName;
            if (AssetDatabase.LoadAssetAtPath<Material>(matPath) == null)
            {
                AssetDatabase.CreateAsset(kMat, matPath);
            }
            dicTotalImageMat.Add(arrShaders[i], AssetDatabase.LoadAssetAtPath<Material>(matPath));
        }

        HandleAllParticleSystems(dicTotalImageMat);
    }

    void HandleAllParticleSystems(Dictionary<string, Material> dicTotalImageMat)
    {
        string relativePath = FileUtil.GetProjectRelativePath(m_arr_Root_Folder_Path[0]);
        fileList.Clear();
        FindAllFilesInFolder(new DirectoryInfo(m_arr_Root_Folder_Path[0]));

        int index = 0;
        foreach (string filePath in fileList)
        {
            if (!filePath.Contains(".meta"))
            {
                string temp = filePath.Replace("\\", "/");
                EditorUtility.DisplayProgressBar("Combine All Prefabs", temp.Substring(temp.LastIndexOf("/") + 1), (index + 1) * 1f / fileList.Count);
                string prefabPath = FileUtil.GetProjectRelativePath(temp);
                GameObject go = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath) as GameObject;
                if (go != null)
                {
                    string kParticleSystemPath = AssetDatabase.GetAssetPath(go);
                    foreach (ParticleSystem mParticleSystem in go.GetComponentsInChildren<ParticleSystem>(true))
                    {
                        ParticleSystem.TextureSheetAnimationModule kModule = mParticleSystem.textureSheetAnimation;
                        if (!kModule.enabled)
                        {
                            ParticleSystemRenderer kParticleRender = (ParticleSystemRenderer)mParticleSystem.GetComponent<Renderer>();
                            if (kParticleRender != null)
                            {
                                Material kMaterial = kParticleRender.sharedMaterial;
                                if (kMaterial != null)
                                {
                                    string materialPath = AssetDatabase.GetAssetPath(kMaterial.GetInstanceID());
                                    Texture kTexture = kMaterial.mainTexture;
                                    if (kTexture != null)
                                    {
                                        string path = AssetDatabase.GetAssetPath(kTexture.GetInstanceID());
                                        Vector4 vctSheet = FindSheetInfo(path);
                                        if (vctSheet[0] != 0 && vctSheet[1] != 0)
                                        {
                                            Shader kShader = kMaterial.shader;
                                            if (kShader != null)
                                            {
                                                if (dicTotalImageMat.ContainsKey(kShader.name))
                                                {
                                                    Material kTargetMat = dicTotalImageMat[kShader.name];
                                                    if (kTargetMat != null)
                                                    {
                                                        kParticleRender.material = kTargetMat;
                                                        kModule.enabled = true;
                                                        kModule.animation = ParticleSystemAnimationType.SingleRow;
                                                        kModule.numTilesX = Mathf.FloorToInt(vctSheet[0]);
                                                        kModule.numTilesY = Mathf.FloorToInt(vctSheet[1]);
                                                        kModule.rowIndex = Mathf.FloorToInt(vctSheet[2]);
                                                        kModule.useRandomRow = false;
                                                        kModule.cycleCount = 1;
                                                        ParticleSystem.MinMaxCurve curve = new ParticleSystem.MinMaxCurve();
                                                        curve.mode = ParticleSystemCurveMode.Constant;
                                                        int iValue = Mathf.FloorToInt(vctSheet[3]);
                                                        curve.constantMax = iValue / vctSheet[0];
                                                        kModule.frameOverTime = curve;
                                                    }
                                                    else
                                                    {
                                                        Debug.LogError("找不到指定shader：" + kShader.name + "的材质球");
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
            index++;
        }
    }

    Vector4 FindSheetInfo(string imageName)
    {
        Vector4 ret = new Vector4();
        string imageSheetInfoFilePath = FileUtil.GetProjectRelativePath(m_arr_Root_Folder_Path[0]) + "/" + "ImageSheetInfo.txt";
        if (File.Exists(imageSheetInfoFilePath))
        {
            string AllSheetCode = File.ReadAllText(imageSheetInfoFilePath);
            string[] tmp = AllSheetCode.Split('\n');
            for (int i = 0; i < tmp.Length; i++)
            {
                if (tmp[i].Contains(imageName))
                {
                    string[] ttmp = tmp[i].Split(':');
                    ret.x = StringtoInteger(ttmp[2]);
                    ret.y = StringtoInteger(ttmp[3]);
                    ret.z = StringtoInteger(ttmp[4]);
                    ret.w = StringtoInteger(ttmp[5]);
                }
            }
        }
        return ret;
    }

    public static int StringtoInteger(string str)
    {
        int i = 0;
        int sign = 0;
        int val = 0;

        while (i < str.Length && ((str[i] >= '0' && str[i] <= '9') || str[i] == ' ' || str[i] == '-' || str[i] == '+'))
        {
            if ((val == 0 && sign == 0) && str[i] == ' ')
                i++;
            else if (str[i] == '-' && sign == 0)
            {
                sign = -1;
                i++;
            }
            else if (str[i] == '+' && sign == 0)
            {
                sign = 1;
                i++;
            }
            else if (str[i] >= '0' && str[i] <= '9')
            {
                //handle overflow, val * 10 + n > int.MaxValue
                if (val > (int.MaxValue - (str[i] - '0')) / 10)
                {
                    if (sign == 0 || sign == 1)
                        return int.MaxValue;
                    return int.MinValue;
                }
                val = val * 10 + str[i] - '0';
                i++;
            }
            else
            {
                if (sign == 0)
                    return val;
                return val * sign;
            }
        }
        if (sign == 0)
            return val;
        return val * sign;
    }
}
