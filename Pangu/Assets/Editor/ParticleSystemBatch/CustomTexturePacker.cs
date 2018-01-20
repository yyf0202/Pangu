using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;

public class CustomTexturePacker
{
	public static List<List<ColorInfo>> CombineTexture( List<Texture2D> list , ref string sheetInfo)
	{
		int totalArea = 0;
		for (int i = 0; i < list.Count; i++)
		{
			if (list [i] != null)
			{
				totalArea += list [i].width * list [i].height;
			}
		}

		int iBaseLengthOfSide = 0;
		for (int i = 2 ;; i*= 2)
		{
			if (totalArea <= i * i)
			{
				iBaseLengthOfSide = i;
				break;
			}
		}
		List<List<ColorInfo>> imageInfos = new List<List<ColorInfo>>();
		while (true)
		{
			if (!GenerateTotalImage (iBaseLengthOfSide, list , ref imageInfos , ref sheetInfo))
			{
				iBaseLengthOfSide *= 2;
			}
			else
			{
				break;
			}
		}

		return imageInfos;
	}

	static bool GenerateTotalImage(int iLengthOfSide , List<Texture2D> list , ref List<List<ColorInfo>> imageInfo , ref string sheetInfo)
	{
		sheetInfo = "";

		list.Sort (delegate(Texture2D x, Texture2D y)
		{
			return - x.width * x.height + y.width * y.height;
		});

		imageInfo.Clear ();
		for (int a = 0; a < iLengthOfSide; a++)
		{
			imageInfo.Add(new List<ColorInfo>());
			for (int b = 0; b < iLengthOfSide; b++)
			{
				ColorInfo kColorInfo = new ColorInfo ();
				kColorInfo.m_iColor = new Color (1f, 1f, 1f, 0f);
				kColorInfo.m_bPained = false;
				imageInfo [a].Add (kColorInfo);
			}
		}

		for (int i = 0; i < list.Count; i++)
		{
			int iPutX = 0;
			int iPutY = 0;
			if(!PutImage(imageInfo , iLengthOfSide , list[i] , ref iPutX , ref iPutY))
			{
				return false;
			}

			string imagePath = AssetDatabase.GetAssetPath (list [i].GetInstanceID ());
			EditorUtility.DisplayProgressBar ("Texture Packing", imagePath, (i+1f) / list.Count);
			if (iPutX % list [i].width != 0 || iPutY % list [i].height != 0 || iLengthOfSide % list [i].width != 0 || iLengthOfSide % list [i].height != 0)
			{
				Debug.LogError ("Error image text sheet info with image path : " + imagePath);
			}
			else
			{
				int dRowNum = iLengthOfSide / list[i].height;
				int dColNum = iLengthOfSide / list[i].width;
				int dColIndex = iPutX / list[i].width;
				int dRowIndex = iPutY / list[i].height;
				sheetInfo += ("imagePath: " + imagePath + ", TextSheet x: " + dColNum + ", y: " + dRowNum + ", rowIndex: " + dRowIndex + ", colIndex: " + dColIndex + "\n");
			}
		}

		return true;
	}

	static bool PutImage(List<List<ColorInfo>> imageInfo , int iLengthOfSide , Texture2D kTexture , ref int iPutX , ref int iPutY)
	{
		for (int a = 0; a < iLengthOfSide; a++)
		{
			for (int b = 0; b < iLengthOfSide; b++)
			{
				if (PutImageHere (imageInfo, a, b, kTexture))
				{
					iPutX = a;
					iPutY = b;
					return true;
				}
			}
		}

		return false;
	}

	static bool PutImageHere(List<List<ColorInfo>> imageInfo , int a , int b , Texture2D kTexture)
	{
		int iTextureWidthCell = kTexture.width;
		int iTextureHeightCell = kTexture.height;

		if (a % iTextureWidthCell != 0 || b % iTextureHeightCell != 0)
		{
			return false;
		}
		for (int i = a; i < a + iTextureWidthCell; i++)
		{
			for (int j = b; j < b + iTextureHeightCell; j++)
			{
				if (imageInfo [i] [j].m_bPained)
				{
					return false;
				}
			}
		}
		for (int i = a; i < a + iTextureWidthCell; i++)
		{
			List<ColorInfo> kList = imageInfo [i];
			for (int j = b; j < b + iTextureHeightCell; j++)
			{
				kList[j].m_bPained = true;
				kList[j].m_iColor = kTexture.GetPixel (i - a, j - b);
			}
		}
		return true;
	}
}

public class ColorInfo
{
	public bool m_bPained;
	public Color m_iColor;
}