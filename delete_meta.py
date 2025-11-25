import os

def main():
    d = os.getcwd()
    for root, _, files in os.walk(d):
        for name in files:
            if name.endswith('.meta'):
                path = os.path.join(root, name)
                if os.path.isfile(path):
                    os.remove(path)

if __name__ == '__main__':
    main()
