import os

from fluent.syntax.parser import FluentParser
from fluent.syntax.serializer import FluentSerializer

from file import YAMLFile, FluentFile
from fluentast import FluentSerializedMessage, FluentAstAttributeFactory
from fluentformatter import FluentFormatter
from project import Project
import logging

######################################### Class definitions ############################################################
class YAMLExtractor:
    def __init__(self, yaml_files):
        self.yaml_files = yaml_files

    def execute(self):
        for yaml_file in yaml_files:
            try:
                # Используем правильный атрибут для получения пути
                file_path = getattr(yaml_file, 'full_path', None) or getattr(yaml_file, 'path', 'unknown')
                logging.info(f"Обработка файла: {file_path}")
                
                yaml_elements = yaml_file.get_elements(yaml_file.parse_data(yaml_file.read_data()))

                if not len(yaml_elements):
                    continue

                fluent_file_serialized = self.get_serialized_fluent_from_yaml_elements(yaml_elements)

                if not fluent_file_serialized:
                    continue

                # Пропускаем создание файлов для en-US
                # pretty_fluent_file_serialized = formatter.format_serialized_file_data(fluent_file_serialized)

                relative_parent_dir = yaml_file.get_relative_parent_dir(project.prototypes_dir_path).lower()
                file_name = yaml_file.get_name()

                # Создаем только русский файл на основе данных из YAML
                ru_fluent_file_path = self.create_ru_fluent_file_from_yaml(relative_parent_dir, file_name, fluent_file_serialized)
                
            except Exception as e:
                # Получаем путь к файлу безопасным способом
                file_path = getattr(yaml_file, 'full_path', None) or getattr(yaml_file, 'path', 'unknown')
                print(f"\n{'='*80}")
                print(f"ОШИБКА В ФАЙЛЕ: {file_path}")
                print(f"Тип ошибки: {type(e).__name__}")
                print(f"Сообщение: {e}")
                print(f"{'='*80}\n")
                
                # Прерываем выполнение при первой ошибке, чтобы увидеть проблемный файл
                raise

    @classmethod
    def serialize_yaml_element(cls, element):
        parent_id = element.parent_id
        if isinstance(parent_id, list):
            parent_id = parent_id[0] if parent_id else 'None'

        message = FluentSerializedMessage.from_yaml_element(
            element.id, element.name,
            FluentAstAttributeFactory.from_yaml_element(element),
            parent_id
        )

        return message

    def get_serialized_fluent_from_yaml_elements(self, yaml_elements):
        fluent_serialized_messages = list(map(YAMLExtractor.serialize_yaml_element, yaml_elements))
        fluent_exist_serialized_messages = list(filter(lambda m: m, fluent_serialized_messages))

        if not len(fluent_exist_serialized_messages):
            return None

        return '\n'.join(fluent_exist_serialized_messages)

    def create_ru_fluent_file_from_yaml(self, relative_parent_dir, file_name, file_data):
        """Создает только русский файл на основе данных из YAML"""
        # Форматируем данные для записи в файл
        pretty_file_data = formatter.format_serialized_file_data(file_data)
        
        # Создаем путь для русского файла
        ru_new_dir_path = os.path.join(project.ru_locale_prototypes_dir_path, relative_parent_dir)
        
        # Создаем директорию, если она не существует
        os.makedirs(ru_new_dir_path, exist_ok=True)
        
        ru_fluent_file_path = os.path.join(ru_new_dir_path, f'{file_name}.ftl')
        ru_fluent_file = FluentFile(ru_fluent_file_path)
        
        # Проверяем, существует ли файл
        if os.path.isfile(ru_fluent_file_path):
            logging.info(f'Файл русской локали уже существует: {ru_fluent_file_path}')
            return ru_fluent_file_path
        else:
            # Создаем новый файл с данными из YAML
            ru_fluent_file.save_data(pretty_file_data)
            logging.info(f'Создан файл русской локали: {ru_fluent_file_path}')
            return ru_fluent_file_path



######################################## Var definitions ###############################################################

logging.basicConfig(level = logging.INFO)
project = Project()
serializer = FluentSerializer()
parser = FluentParser()
formatter = FluentFormatter()

yaml_files_paths = project.get_files_paths_by_dir(project.prototypes_dir_path, 'yml')
yaml_files = list(map(lambda yaml_file_path: YAMLFile(yaml_file_path), yaml_files_paths))

########################################################################################################################

logging.info(f'Поиск yaml-файлов ...')
YAMLExtractor(yaml_files).execute()