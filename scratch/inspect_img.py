from PIL import Image

path = r"C:\Users\27487\.gemini\antigravity-ide\brain\afc973be-1aa4-4926-8fa3-55007508e07a\.user_uploaded\media_1790093496323.png"
im = Image.open(path)
print("Image size:", im.size)
