using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class CameraEffectManager : MonoBehaviour
{

    #region Private Members
    private CommandBuffer command;
    private bool addCommand = false;
    private Dictionary<GameObject, Renderer[]> renderDic = new Dictionary<GameObject, Renderer[]>();
    private Camera m_mainCam;
    private Material m_outlineMat;
    #endregion

    #region Public Members
    public float qualityOrighScene = 0.5f;
    public float qualityBlur = 0.25f;
    public Color outterLineColor = Color.red;
    public float outterLineSize = 1;
    #endregion

    protected void Awake()
    {
        m_mainCam = this.GetComponent<Camera>();
        m_mainCam.backgroundColor = new Color(0, 0, 0, 0);
        m_outlineMat = new Material(Shader.Find("Custom/Effect/Outline"));
        m_outlineMat.hideFlags = HideFlags.HideAndDontSave;

    }

    protected void OnDestory()
    {
        DestroyMatAndRT();
    }

    void OnDisable()
    {
        ResetCommandBuffer();
    }

    private void DestroyMatAndRT()
    {
        if (m_outlineMat)
            DestroyImmediate(m_outlineMat);
    }

    private void ResetCommandBuffer()
    {
        if (command != null)
            command.Clear();
    }

    public void AddOutterlineTarget(GameObject go)
    {
        if (go == null)
            return;

        Renderer[] renderers = go.GetComponentsInChildren<Renderer>();
        if (renderers.Length == 0)
        {
            return;
        }

        if (renderDic.Count == 0)
            addCommand = true;

        renderDic.Add(go, renderers);
    }

    public void RemoveOutterlineTarget(GameObject go)
    {
        renderDic.Remove(go);

        if (renderDic.Count == 0)
        {
            addCommand = false;
        }
    }

    private void OnPreRender()
    {
        var cam = Camera.current;
        if (cam == null && cam != m_mainCam)
            return;

        if (!addCommand)
        {
            return;
        }

        if (command == null)
        {
            command = new CommandBuffer();
            command.name = "Command Buffer Outline";
            m_mainCam.AddCommandBuffer(CameraEvent.AfterEverything, command);
        }

        int sceneId = 0;
        int blur1 = 1;
        int blur2 = 2;
        int screenCopy = 3;

        command.Clear();
        command.GetTemporaryRT(sceneId, (int)(m_mainCam.pixelWidth * qualityOrighScene), (int)(m_mainCam.pixelHeight * qualityOrighScene), 0, FilterMode.Bilinear, RenderTextureFormat.ARGB32);
        command.GetTemporaryRT(screenCopy, -1, -1, 0, FilterMode.Bilinear);
        command.Blit(BuiltinRenderTextureType.CurrentActive, screenCopy);
        command.SetRenderTarget(sceneId);
        command.ClearRenderTarget(true, true, Color.clear);
        command.GetTemporaryRT(blur1, (int)(m_mainCam.pixelWidth * qualityBlur), (int)(m_mainCam.pixelHeight * qualityBlur), 0, FilterMode.Bilinear, RenderTextureFormat.ARGB32);
        command.GetTemporaryRT(blur2, (int)(m_mainCam.pixelWidth * qualityBlur), (int)(m_mainCam.pixelHeight * qualityBlur), 0, FilterMode.Bilinear, RenderTextureFormat.ARGB32);
        command.SetGlobalColor("Outline_Color", outterLineColor);

        foreach (Renderer[] renders in renderDic.Values)
            for (int i = 0; i < renders.Length; ++i)
                if (renders[i] != null && renders[i].isVisible)
                    command.DrawRenderer(renders[i], m_outlineMat);

        //3. Blur
        command.SetGlobalVector("offsets", new Vector4(outterLineSize / Screen.width, 0, 0, 0));
        command.Blit(sceneId, blur1, m_outlineMat, 1);
        command.SetGlobalVector("offsets", new Vector4(0, outterLineSize / Screen.height, 0, 0));
        command.Blit(blur1, blur2, m_outlineMat, 1);

        //4. Cut off
        command.Blit(sceneId, blur2, m_outlineMat, 2);
        command.SetGlobalTexture("g_BlurSilhouette", blur2);
        command.Blit(screenCopy, BuiltinRenderTextureType.CameraTarget, m_outlineMat, 3);
        command.ReleaseTemporaryRT(blur1);
        command.ReleaseTemporaryRT(blur2);
        command.ReleaseTemporaryRT(sceneId);
        command.ReleaseTemporaryRT(screenCopy);

    }
}
