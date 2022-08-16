from os import listdir, rename
location = "Prefabs\FrogPath\\"
offset = 0


def filteredList(file):
    if "Brick" in file:
        return True


filtered = filter(filteredList, listdir(location))


for fileName in filtered:
    print(fileName)
    newFileName = fileName.replace("Brick", "Frog")
    print("          " + newFileName)
    rename(location+fileName, location+newFileName)