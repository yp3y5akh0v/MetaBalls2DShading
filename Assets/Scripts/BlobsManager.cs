using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(BlobController))]
public class BlobsManager : MonoBehaviour
{
    [Range(0.1f, 100f)]
    public float blobsMaxRadius;

    [Range(0f, 100f)]
    public float blobsMaxVelocity;

    [Range(0.1f, 100f)]
    public float blobsMaxMass;

    public bool gravity;

    [Range(0f, 100f)]
    public float gCoef;

    [Range(0.01f, 100f)]
    public float epsPosForce;

    [Range(0f, 0.1f)]
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

    void Start()
    {
        rect = GetComponent<RectTransform>().rect;
        material = GetComponent<RawImage>().material;
    }

    void FixedUpdate()
    {
        InputHandler();
        AdjustBlobsVelocity();
        AdjustBlobsPosition();
        DesorbeBlobsMass();
        CompressBlobsRadius();
        ApplySpaceBlobsFriction();
        fixArrayIndices();
        UpdateMaterial();
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
        while(i < blobsCount)
        {
            if (!blobs[i].GetComponent<BlobController>().isAlive)
            {
                var curBlob = blobs[i];
                for(int j = i + 1; j < blobsCount; j++)
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

            Vector2 totalAcceleration = Vector2.zero;
            for (int j = 0; j < blobsCount; j++)
            {
                if (i != j)
                {
                    var otherBlob = blobs[j];
                    Vector2 dir = new Vector2(otherBlob.transform.localPosition.x - curBlob.transform.localPosition.x,
                                            otherBlob.transform.localPosition.y - curBlob.transform.localPosition.y);
                    if (dir.magnitude > epsPosForce)
                    {
                        totalAcceleration += 1e+4f * gCoef * otherBlob.GetComponent<BlobController>().mass * dir.normalized / dir.sqrMagnitude;
                    }

                    float deltaPos = dir.magnitude - (curBlob.GetComponent<BlobController>().radius + otherBlob.GetComponent<BlobController>().radius);
                    if (deltaPos < 0)
                    {
                        curBlob.GetComponent<BlobController>().AbsorbeMass();
                        curBlob.GetComponent<BlobController>().ExpanseRadius();
                        otherBlob.GetComponent<BlobController>().AbsorbeMass();
                        otherBlob.GetComponent<BlobController>().ExpanseRadius();
                    }
                }
            }
            if (gravity)
            {
                curBlob.GetComponent<BlobController>().UpdateVelocity(totalAcceleration);
            }
            curBlob.GetComponent<BlobController>().AdjustVelocity(rect);
        }
    }

    private void AdjustBlobsPosition()
    {
        for (int i = 0; i < blobsCount; i++)
        {
            blobs[i].GetComponent<BlobController>().AdjustPosition(rect);
        }
    }

    private void InputHandler()
    {
        if (Input.GetKeyDown(KeyCode.KeypadPlus) && blobsCount < 100)
        {
            GameObject blob = Instantiate(blobPrefab, 
                new Vector3(Random.Range(-rect.width / 2, rect.width / 2), Random.Range(-rect.height / 2, rect.height / 2), transform.parent.position.z),
                Quaternion.identity);
            blob.GetComponent<BlobController>().velocity = new Vector2(Random.value, Random.value).normalized * Random.Range(0, blobsMaxVelocity);
            blob.GetComponent<BlobController>().radius = Random.Range(0, blobsMaxRadius);
            blob.GetComponent<BlobController>().mass = Random.Range(0, blobsMaxMass);
            blob.GetComponent<BlobController>().massAbsorbingRate = Random.Range(0f, 0.01f);
            blob.GetComponent<BlobController>().massDesorbingRate = Random.Range(0f, 0.01f);
            blob.GetComponent<BlobController>().radiusCompressionRate = Random.Range(0f, 0.01f);
            blob.GetComponent<BlobController>().radiusExpansionRate = Random.Range(0f, 0.01f);
            blob.GetComponent<BlobController>().isAlive = true;
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
