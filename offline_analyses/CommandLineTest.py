import argparse

# Initialize parser
parser = argparse.ArgumentParser()

# Adding optional argument
parser.add_argument("-s", "--showResultsBoolean", help="Show Output")

# Read arguments from command line
args = parser.parse_args()

if args.showResultsBoolean:
    print("showResultsBoolean: % s" % args.showResultsBoolean)

    def str2bool(v):
        return v.lower() in "true"

    isShowResults = str2bool(args.showResultsBoolean)
    print(type(isShowResults))
else:
    print("use flags -h, --help to show help")
    isShowResults = True
    print(type(isShowResults))

print(str.format("isShowResults: {}", isShowResults))

if isShowResults:
    print(True)
else:
    print(False)