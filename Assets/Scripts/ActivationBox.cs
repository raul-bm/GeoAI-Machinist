using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using TMPro;
using UnityEngine;

public class ActivationBox : MonoBehaviour
{
    public Action OnGrabbed;

    public Action<string> OnHover;
    public Action<string> OnUnhover;

    public bool grabbed = false;

    // Blink
    public GameObject outline;
    bool outlineBlinking = false;
    float blinkPeriodSeconds = 1;
    float timer = 0f;

    // Data
    string type;
    private int id = -1;

    [SerializeField] private GameObject spaceHint;

    public void Init(int newId)
    {
        id = newId;
        gameObject.name = "ActivationBox" + id;
    }

    public int GetId()
    {
        return id;
    }
    public void SetFunction(string newType)
    {
        type = newType;
        Draw();
    }

    void Draw()
    {
        // TODO: Update Sprite to represent the correspoding function
        Transform label = transform.Find("Label");
        label.GetComponent<TextMeshPro>().text = type;
    }

    public void Grab(Vector3 grabberPosition)
    {
        grabbed = true;
        Vector3 relativeToParentPosition = new(0f, 0f, 0f);
        transform.localPosition = relativeToParentPosition;
        OnGrabbed?.Invoke();
        StopBlink();

        spaceHint.SetActive(false);
    }

    public bool IsGrabbed()
    {
        return grabbed;
    }

    public void PlaceAt(Vector3 newPosition)
    {
        transform.parent = null;
        transform.position = newPosition;
    }

    public void Block()
    {
        transform.tag = "Untagged"; // Player cannot grab Untagged objects
    }

    public void Release()
    {
        transform.tag = "ActivationBox";
    }

    public double ApplyFunction(double pixelValue)
    {
        switch (type)
        {
            case "ReLu":
                return ReLu(pixelValue);
            case "Sigmoid":
                return Sigmoid(pixelValue);
            case "tanh":
                return HyperbolicTangent(pixelValue);
            case "Linear":
                return Linear(pixelValue);
        }
        return pixelValue;
    }

    double ReLu(double value)
    {
        if (value > 0)
        {
            return value;
        }

        return 0;
    }

    double Sigmoid(double value)
    {
        return 1.0f / (1.0f + (float)Math.Exp(-value));
    }

    double HyperbolicTangent(double x)
    {
        if (x < -45.0) return -1.0;
        else if (x > 45.0) return 1.0;
        else return Math.Tanh(x);
    }

    double Linear(double x)
    {
        return x;
    }

    public void Blink()
    {
        outlineBlinking = true;
    }

    public void StopBlink()
    {
        outlineBlinking = false;
        outline.SetActive(false);
    }

    private void ToggleKernelOutline()
    {
        outline.SetActive(!outline.activeSelf);
    }

    void Update()
    {
        if (outlineBlinking)
        {
            timer += Time.deltaTime;
            if (timer >= blinkPeriodSeconds)
            {
                ToggleKernelOutline();
                timer = 0f;
            }
        }

        if (grabbed) OnUnhover?.Invoke(type);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            GameObject grabbedObject = other.GetComponent<PlayerController>().grabbedObject;

            if (grabbedObject == null) spaceHint.SetActive(true);
        }

        if (!grabbed) OnHover?.Invoke(type);
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            GameObject grabbedObject = other.GetComponent<PlayerController>().grabbedObject;

            if (grabbedObject == null) spaceHint.SetActive(false);
        }

        if (!grabbed) OnUnhover?.Invoke(type);
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        if (!grabbed) OnHover?.Invoke(type);
    }

}
