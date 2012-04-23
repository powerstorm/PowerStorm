class FixColumnNameBuildingType < ActiveRecord::Migration
  def self.up
    rename_column :buildings, :type, :building_type
  end

  def self.down
  end
end
