using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(BlobController))]
public class BlobsManager : MonoBehaviour
{
    [Range(0f, 100f)]
    public float blobsInitVelocity;

    [Range(0.01f, 10000f)]
    public float blobsMaxVelocity;

    [Range(0.1f, 100f)]
    public float blobsInitRadius;

    [Range(0.01f, 500f)]
    public float blobsMaxRadius;
    
    [Range(0.1f, 100f)]
    public float blobsInitMass;

    [Range(0.01f, 10000f)]
    public float blobsMaxMass;

    public bool initGravity;

    [Range(0f, 100f)]
    public float gCoef;

    [Range(0.01f, 100f)]
    public float epsPosForce;

    [Range(0f, 1f)]
    public float spaceFriction;

    public GameObject blobPrefab;

    private GameObject[] blobs = new GameObject[100];
    private int blobsCount = 0;
    private Rect rect;
    private Material material;

    // Raw arrays for material
    private float[] r = new float[100];
    private Vector4[] p = new Vector4[100];
    private float[] m = new float[100];

    [Range(0.0001f, 1f)]
    public float scaleRect;

    void Start()
    {
        rect = GetComponent<RectTransform>().rect;
        material = GetComponent<RawImage>().material;
    }

    void FixedUpdate()
    {
        InputHandler();
        AdjustScalerFactor();
        AdjustBlobsVelocity();
        AdjustBlobsPosition();
        DesorbeBlobsMass();
        CompressBlobsRadius();
        ApplySpaceBlobsFriction();
        fixArrayIndices();
        UpdateMaterial();
    }

    private void AdjustScalerFactor()
    {
        GameObject canv = transform.parent.gameObject;
        canv.GetComponent<CanvasScaler>().scaleFactor = scaleRect;
    }

    private void DesorbeBlobsMass()
    {
        for (int i = 0; i < blobsCount; i++)
        {
            blobs[i].GetComponent<BlobController>().DesorbeMass();
        }
    }

    private void CompressBlobsRadius()
    {
        for (int i = 0; i < blobsCount; i++)
        {
            blobs[i].GetComponent<BlobController>().CompressRadius();
        }
    }

    private void ApplySpaceBlobsFriction()
    {
        for (int i = 0; i < blobsCount; i++)
        {
            blobs[i].GetComponent<BlobController>().applyFriction(spaceFriction);
        }
    }

    private void fixArrayIndices()
    {
        int i = 0;
        while (i < blobsCount)
        {
            if (!blobs[i].GetComponent<BlobController>().isAlive)
            {
                var curBlob = blobs[i];
                curBlob.SetActive(false);

                for (int j = i + 1; j < blobsCount; j++)
                {
                    blobs[j - 1] = blobs[j];
                }

                --blobsCount;
                Destroy(curBlob);
            }
            i++;
        }
    }

    private void UpdateMaterial()
    {
        for (int i = 0; i < 100; i++)
        {
            if (i < blobsCount)
            {
                r[i] = blobs[i].GetComponent<BlobController>().radius;
                p[i] = new Vector4(blobs[i].transform.localPosition.x,
                    blobs[i].transform.localPosition.y,
                    blobs[i].transform.localPosition.z,
                    0);
                m[i] = blobs[i].GetComponent<BlobController>().mass;
            }
            else
            {
                r[i] = 0f;
                p[i] = Vector4.zero;
                m[i] = 0f;
            }
        }

        material.SetInt("blobsCount", blobsCount);
        material.SetFloatArray("blobsRadius", r);
        material.SetVectorArray("blobsPosition", p);
        material.SetFloatArray("blobsMass", m);
    }

    private void AdjustBlobsVelocity()
    {
        for (int i = 0; i < blobsCount; i++)
        {
            var curBlob = blobs[i];
            var totalAcceleration = Vector3.zero;
            for (int j = 0; j < blobsCount; j++)
            {
                if (i != j)
                {
                    var otherBlob = blobs[j];
                    var dir = otherBlob.transform.localPosition - curBlob.transform.localPosition;

                    if (dir.magnitude > epsPosForce && otherBlob.GetComponent<BlobController>().localGravity)
                    {
                        totalAcceleration += 1e+4f * gCoef * otherBlob.GetComponent<BlobController>().mass * dir.normalized / dir.sqrMagnitude;
                    }

                    float deltaPos = DeltaPosCalculate(curBlob, otherBlob);
                    if (deltaPos < 0f)
                    {
                        curBlob.GetComponent<BlobController>().AbsorbeMass(blobsMaxMass);
                        curBlob.GetComponent<BlobController>().ExpanseRadius(blobsMaxRadius);
                        otherBlob.GetComponent<BlobController>().AbsorbeMass(blobsMaxMass);
                        otherBlob.GetComponent<BlobController>().ExpanseRadius(blobsMaxRadius);
                    }
                }
            }
            curBlob.GetComponent<BlobController>().UpdateVelocity(totalAcceleration, blobsMaxVelocity);
            curBlob.GetComponent<BlobController>().AdjustVelocity(rect, scaleRect);
        }
    }

    private float DeltaPosCalculate(GameObject a, GameObject b)
    {
        var dir = b.transform.localPosition - a.transform.localPosition;
        return dir.magnitude - (a.GetComponent<BlobController>().radius + b.GetComponent<BlobController>().radius);
    }

    private void AdjustBlobsPosition()
    {
        for (int i = 0; i < blobsCount; i++)
        {
            blobs[i].GetComponent<BlobController>().AdjustPosition(rect, scaleRect);
        }
    } 

    private void InputHandler()
    {
        if (Input.GetKeyDown(KeyCode.KeypadPlus) && blobsCount < 100)
        {
            GameObject blob = Instantiate(blobPrefab, 
                new Vector3(Random.Range(-rect.width / 2, rect.width / 2), Random.Range(-rect.height / 2, rect.height / 2), transform.parent.position.z),
                Quaternion.identity);
            blob.GetComponent<BlobController>().velocity = new Vector3(Random.value, Random.value, 0f).normalized * Random.Range(0, blobsInitVelocity);
            blob.GetComponent<BlobController>().radius = Random.Range(0, blobsInitRadius);
            blob.GetComponent<BlobController>().mass = Random.Range(0, blobsInitRadius);
            blob.GetComponent<BlobController>().massAbsorbingRate = Random.Range(0f, 0.01f);
            blob.GetComponent<BlobController>().massDesorbingRate = Random.Range(0f, 0.01f);
            blob.GetComponent<BlobController>().radiusCompressionRate = Random.Range(0f, 0.01f);
            blob.GetComponent<BlobController>().radiusExpansionRate = Random.Range(0f, 0.01f);
            blob.GetComponent<BlobController>().isAlive = true;
            blob.GetComponent<BlobController>().localGravity = initGravity;
            blob.transform.localScale *= blob.GetComponent<BlobController>().radius;
            blob.transform.parent = transform;
            blobs[blobsCount++] = blob;
        }

        if (Input.GetKeyDown(KeyCode.KeypadMinus) && blobsCount > 0)
        {
            Destroy(blobs[--blobsCount]);
        }
    }
}
