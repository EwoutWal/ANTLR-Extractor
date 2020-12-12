with open("git-urls.txt", "r") as f:
    lines = f.readlines()
    formattedLines = [line.replace("\n", "") for line in lines]

linesSet = set(formattedLines)

with open("git-urls-normalized", "w") as f:
    uniquesList = ["%s\n" % line for line in list(linesSet)]
    f.writelines(uniquesList)